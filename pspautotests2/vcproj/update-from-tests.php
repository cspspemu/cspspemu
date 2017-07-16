<?php

// Instructions:
// Run this with -n -v and it will tell you what it will do.
// Then omit the -n to modify the project file.

// TODO: Add a fallback to generate our own?
if (!dl('php_com_dotnet.dll'))
{
	echo 'Please make sure you have PHP 5.3.x or higher with com_dotnet.', "\n";
	echo 'You can get it from http://windows.php.net/.', "\n";
	exit(1);
}

// Print out some verbose messages.
define('VERBOSE', in_array('-v', $argv));
// Don't actually write the vcxproj/filters files.
define('DRY_RUN', in_array('-n', $argv));

define('GIT', 'git');

abstract class MSProjectFile
{
	protected $filename;
	protected $base;
	protected $data;

	public function __construct($filename)
	{
		$this->base = dirname(realpath($filename));
		$this->filename = $filename;
		$this->reload();
	}

	public function reload()
	{
		$this->data = simplexml_load_file($this->filename);
	}

	protected function categorizeFile($file)
	{
		$file = str_replace(dirname($this->base), '..', realpath($file));
		$filter = dirname(substr($file, 3));
		if (!$this->hasFilter($filter) && substr_count($filter, '\\') >= 2)
			$filter = dirname($filter);

		$ext = substr(strrchr($file, '.'), 1);
		if ($ext === 'c' || $ext === 'S')
			$type = 'ClCompile';
		elseif ($ext === 'h')
			$type = 'ClInclude';
		elseif ($ext === 'mak' || $ext === 'txt' || $ext === 'info')
			$type = 'None';
		elseif ($ext === 'expected')
		{
			$filter .= '\\expected';
			$type = 'None';
		}
		// TODO: Add other interesting extensions if needed.
		else
		{
			echo 'WARN: skipping unknown filetype: ', $file, "\n";
			return false;
		}

		return compact('file', 'type', 'filter');
	}

	protected function hasFilter($filter)
	{
		return false;
	}

	public function save()
	{
		if (!DRY_RUN)
		{
			$xml = $this->data->asXML();

			// Correct some differences between SimplXML and what Visual Studio writes out.
			$xml = "\xEF\xBB\xBF" . trim(str_replace('"/>', '" />', $xml));

			// Yeah, these are ugly, sorry.  Would rather not add the whitespace in the DOM.
			$xml = str_replace('></ItemGroup>', '>' . "\n" . '  </ItemGroup>', $xml);
			// Ident ClInclude/ClCompileNone inside ItemGroup two spaces.
			$xml = preg_replace('~\n  \<(ClInclude|ClCompile|None) ~', "\n" . '    <$1 ', $xml);
			$xml = preg_replace('~\>\<(ClInclude|ClCompile|None) ~', '>' . "\n" . '    <$1 ', $xml);
			// Indent Filter two spaces, the VC way.
			$xml = str_replace('><Filter>', '>' . "\n" . '      <Filter>', $xml);
			$xml = str_replace('</Filter><', '</Filter>' . "\n" . '    <', $xml);

			$xml = str_replace("\n", "\r\n", str_replace("\r", '', $xml));

			file_put_contents($this->filename, $xml);
		}
	}
}

class FiltersFile extends MSProjectFile
{
	protected $filters;
	protected $files;
	protected $mapping;

	public function reload()
	{
		// TODO: Actually do this right.  For now, not expecting VS to change the order.
		// Doing this right means detecting what types are already in the ItemGroup and classifying that way.
		$this->mapping = array(
			'None' => 1,
			'ClInclude' => 2,
			'ClCompile' => 3,
		);

		parent::reload();

		$this->filters = array();
		foreach ($this->data->ItemGroup[0]->Filter as $filter)
			$this->filters[(string) $filter['Include']] = (string) $filter->UniqueIdentifier;

		$this->files = array();
		foreach ($this->mapping as $name => $place)
		{
			$this->files[$name] = array();
			foreach ($this->data->ItemGroup[$place]->$name as $file)
				$this->files[$name][(string) $file['Include']] = (string) $file->Filter;
		}
	}

	public function add($file)
	{
		$info = $this->categorizeFile($file);
		if ($info === false)
			return;

		$this->addFilter($info['filter']);
		$added = $this->addFile($info['file'], $info['type'], $info['filter']);

		if (VERBOSE && $added)
			echo $info['file'], ' (', $info['type'], ') => ', $info['filter'], "\n";

		return $added;
	}

	public function addFile($file, $type, $filter)
	{
		if (isset($this->files[$type][$file]))
			return false;

		$place = $this->mapping[$type];
		$node = $this->data->ItemGroup[$place]->addChild($type);
		$node['Include'] = $file;
		$node->Filter = $filter;

		$this->files[$type][$file] = $filter;

		return true;
	}

	public function addFilter($filter)
	{
		$current = strtok($filter, '\\');
		$this->addExactFilter($current);
		while (($tok = strtok('\\')) !== false)
		{
			$current .= '\\' . $tok;
			$this->addExactFilter($current);
		}
	}

	public function addExactFilter($filter)
	{
		if (isset($this->filters[$filter]))
			return false;

		$guid = com_create_guid();
		$node = $this->data->ItemGroup[0]->addChild('Filter');
		$node['Include'] = $filter;
		$node->UniqueIdentifier = $guid;

		$this->filters[$filter] = $guid;
		return true;
	}

	protected function hasFilter($filter)
	{
		return isset($this->filters[$filter]);
	}
}

class VCXProjectFile extends MSProjectFile
{
	protected $files;

	public function reload()
	{
		// TODO: Actually do this right.  For now, not expecting VS to change the order.
		$this->mapping = array(
			'None' => 2,
			'ClInclude' => 3,
			'ClCompile' => 1,
		);

		parent::reload();

		$this->files = array();
		foreach ($this->mapping as $name => $place)
		{
			$this->files[$name] = array();
			foreach ($this->data->ItemGroup[$place]->$name as $file)
				$this->files[$name][(string) $file['Include']] = true;
		}
	}

	public function add($file)
	{
		$info = $this->categorizeFile($file);
		if ($info === false)
			return;

		return $this->addFile($info['file'], $info['type']);
	}

	public function addFile($file, $type)
	{
		if (isset($this->files[$type][$file]))
			return false;

		$place = $this->mapping[$type];
		$node = $this->data->ItemGroup[$place]->addChild($type);
		$node['Include'] = $file;

		$this->files[$type][$file] = true;
		return true;
	}
}

$old = getcwd();
chdir(__DIR__);

if (!file_exists('pspautotests.vcxproj'))
{
	echo 'Oops, where\'s pspautotests.vcxproj?', "\n";
	exit(1);
}

$proj = new VCXProjectFile('pspautotests.vcxproj');
$filters = new FiltersFile('pspautotests.vcxproj.filters');

process_dir('../tests', $proj, $filters);

$proj->save();
$filters->save();

chdir($old);

function process_dir($path, $proj, $filters)
{
	$paths = git_files($path);

	// Git not working / not installed?  Warning: this gives us full paths.
	if (empty($paths))
	{
		// Intentionally not doing this when not VERBOSE.
		if (VERBOSE)
		{
			echo 'WARN: git ls-files failed, falling back.', "\n";
			$paths = fallback_files($path);
		}
		else
			echo 'WARN: git ls-files failed.', "\n";
	}

	// Sort by length descending so we categorize better.
	usort($paths, function ($a, $b)
	{
		return strlen($b) - strlen($a);
	});

	$paths = array_reverse($paths);
	foreach ($paths as $path)
	{
		if (is_dir($path))
			continue;

		$basename = basename($path);
		if (substr($basename, 0, 1) === '.')
			continue;

		// TODO: Externalize this list somehow?
		if (preg_match('~\.(mak|o|elf|prx|compile|PBP|SFO|expected_|pmf|vag|exp|iso|tga|at3|bat|php|sh|pgf|map|png|bin)$~', $basename) != 0)
			continue;
		if ($basename === 'systemparam' || $basename === 'build.txt' || $basename === 'Makefile')
			continue;

		$filters->add((string) $path);
		$proj->add((string) $path);
	}
}

function git_files($path)
{
	$null = PHP_OS == 'WINNT' ? 'NUL' : '/dev/null';

	// The idea here is to make git do the work of excludes/.gitignore/etc.
	$files_listing = @shell_exec(GIT . ' ls-files ' . escapeshellarg($path) . ' 2>' . $null);
	return array_filter(array_map('trim', explode("\n", $files_listing)));
}

function fallback_files($path)
{
	$it = new RecursiveIteratorIterator(new RecursiveDirectoryIterator($path));

	$paths = array();
	foreach ($it as $path)
		$paths[] = (string) $path;

	return $paths;
}

?>