# Utility to make it slightly easier to get the results from a test back
# and put it as an "expected" file.

import sys
import io
import os
import subprocess
import threading
import shutil
import time
import re
import socket

PSPSH = "pspsh"
HOSTFS = "usbhostfs_pc"
MAKE = "make"
TEST_ROOT = "tests/"
PORT = 3000
OUTFILE = "__testoutput.txt"
OUTFILE2 = "__testerror.txt"
FINISHFILE = "__testfinish.txt"
SHOTFILE = "__screenshot.bmp"
TIMEOUT = 10
RECONNECT_TIMEOUT = 6

hostfs_command = None

tests_to_generate = [
  "cpu/cpu_alu/cpu_alu",
  "cpu/vfpu/base",
  "cpu/vfpu/convert/vfpu_convert",
  "cpu/vfpu/prefixes",
  "cpu/vfpu/colors/vfpu_colors",
  "cpu/icache/icache",
  "cpu/lsu/lsu",
  "cpu/fpu/fpu",

  "ctrl/ctrl",
  "ctrl/idle/idle",
  "ctrl/sampling/sampling",
  "ctrl/sampling2/sampling2",
  "display/display",
  "dmac/dmactest",
  "loader/bss/bss",
  "intr/intr",
  "intr/vblank/vblank",
  "misc/testgp",
  "string/string",
  "gpu/callbacks/ge_callbacks",
  "gpu/displaylist/state",
  "threads/alarm/alarm",
  "threads/alarm/cancel/cancel",
  "threads/alarm/refer/refer",
  "threads/alarm/set/set",
  "threads/events/events",
  "threads/events/cancel/cancel",
  "threads/events/clear/clear",
  "threads/events/create/create",
  "threads/events/delete/delete",
  "threads/events/poll/poll",
  "threads/events/refer/refer",
  "threads/events/set/set",
  "threads/events/wait/wait",
  "threads/lwmutex/create/create",
  "threads/lwmutex/delete/delete",
  "threads/lwmutex/lock/lock",
  "threads/lwmutex/priority/priority",
  "threads/lwmutex/try/try",
  "threads/lwmutex/try600/try600",
  "threads/lwmutex/unlock/unlock",
  "threads/mbx/mbx",
  "threads/mbx/cancel/cancel",
  "threads/mbx/create/create",
  "threads/mbx/delete/delete",
  "threads/mbx/poll/poll",
  "threads/mbx/priority/priority",
  "threads/mbx/receive/receive",
  "threads/mbx/refer/refer",
  "threads/mbx/send/send",
  "threads/mutex/mutex",
  "threads/mutex/create/create",
  "threads/mutex/delete/delete",
  "threads/mutex/lock/lock",
  "threads/mutex/priority/priority",
  "threads/mutex/try/try",
  "threads/mutex/unlock/unlock",
  "threads/semaphores/semaphores",
  "threads/semaphores/cancel/cancel",
  "threads/semaphores/create/create",
  "threads/semaphores/delete/delete",
  "threads/semaphores/poll/poll",
  "threads/semaphores/priority/priority",
  "threads/semaphores/refer/refer",
  "threads/semaphores/signal/signal",
  "threads/semaphores/wait/wait",
  "power/power",
  "umd/callbacks/umd",
  "umd/wait/wait",
  "io/directory/directory",
]

# This list is probably not at all correct.
all_versions = {
  "1.00": ["--sdkver=1000010"],
  "1.50": ["--sdkver=1050010"],
  "2.00": ["--sdkver=2000010"],
  "2.50": ["--sdkver=2050010"],
  "2.60": ["--sdkver=2060010"],
  "2.70": ["--sdkver=2070010"],
  "2.80": ["--sdkver=2080010"],
  "3.00": ["--sdkver=3000010"],
  "3.10": ["--sdkver=3010010"],
  "3.30": ["--sdkver=3030010"],
  "3.40": ["--sdkver=3040010"],
  "3.50": ["--sdkver=3050010"],
  "3.60": ["--sdkver=3060010"],
  "3.70": ["--sdkver=3070010", "--sdkver-func=370"],
  "3.80": ["--sdkver=3080010", "--sdkver-func=380"],
  "3.90": ["--sdkver=3090010", "--sdkver-func=380"],
  "3.95": ["--sdkver=3090510", "--sdkver-func=395"],
  "3.96": ["--sdkver=3090610", "--sdkver-func=395"],
  "4.00": ["--sdkver=4000010", "--sdkver-func=395"],
  "4.01": ["--sdkver=4000110", "--sdkver-func=395"],
  "4.05": ["--sdkver=4000510", "--sdkver-func=395"],
  "5.00": ["--sdkver=5000010", "--sdkver-func=500"],
  "5.10": ["--sdkver=5010010", "--sdkver-func=500"],
  "5.20": ["--sdkver=5020010", "--sdkver-func=500"],
  "5.30": ["--sdkver=5030010", "--sdkver-func=500"],
  "5.40": ["--sdkver=5040010", "--sdkver-func=500"],
  "5.50": ["--sdkver=5050010", "--sdkver-func=500"],
  "5.70": ["--sdkver=5070010", "--sdkver-func=507"],
  "6.00": ["--sdkver=6000010", "--sdkver-func=600"],
  "6.10": ["--sdkver=6010010", "--sdkver-func=600"],
  "6.20": ["--sdkver=6020010", "--sdkver-func=600"],
  "6.30": ["--sdkver=6030010", "--sdkver-func=603"],
  "6.40": ["--sdkver=6040010", "--sdkver-func=603"],
  "6.50": ["--sdkver=6050010", "--sdkver-func=603"],
  "6.60": ["--sdkver=6060010", "--sdkver-func=606"],
}

class Command(object):
  def __init__(self, cmd):
    self.cmd = cmd
    self.process = None
    self.output = None
    self.timeout = False
    self.thread = None

  def start(self, capture=True):
    def target():
      self.process = subprocess.Popen(self.cmd, stdin=subprocess.PIPE, stdout=subprocess.PIPE)
      if capture:
        self.output, _ = self.process.communicate()

    self.thread = threading.Thread(target=target)
    self.thread.start()

  def stop(self):
    if self.thread.is_alive():
      self.timeout = True
      try:
        self.process.terminate()
      except WindowsError:
        print "Could not terminate process"
      self.thread.join()

  def run(self, timeout):
    self.start()
    self.thread.join(timeout)

def wait_until(predicate, timeout, interval):
  mustend = time.time() + timeout
  while time.time() < mustend:
    if predicate(): return True
    time.sleep(interval)
  return False

def pspsh_is_ready():
  c = Command([PSPSH, "-p", str(PORT), "-e", "ls"])
  c.run(0.5)
  if c.output == None:
    return False
  return c.output.count("\n") > 2

def hostfs_is_ready():
  s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
  try:
    s.connect(("127.0.0.1", PORT))
    s.close()
    return True
  except socket.error:
    return False

def start_hostfs():
  hostfs_command = Command([HOSTFS, "-b", str(PORT)])
  hostfs_command.start(capture=False)

def stop_hostfs():
  if hostfs_command != None:
    hostfs_command.stop()

def init():
  if not os.path.exists(TEST_ROOT + "../common/libcommon.a"):
    print "Please install the pspsdk and run make in common/"
    if not ("-k" in sys.argv or "--keep" in sys.argv):
      sys.exit(1)

def prepare_test(test, args):
  if not ("-k" in args or "--keep" in args):
    olddir = os.getcwd()
    os.chdir(TEST_ROOT + os.path.dirname(test))

    make_target = "all"
    if "-r" in args or "--rebuild" in args:
      make_target = "rebuild"

    make_result = os.system("%s MAKE=\"%s\" %s" % (MAKE, MAKE, make_target))
    os.chdir(olddir)

    # Don't run the test if make failed, let them fix it.
    if make_result > 0:
      sys.exit(make_result)

def gen_test(test, args):
  if os.path.exists(OUTFILE):
    os.unlink(OUTFILE)
  if os.path.exists(OUTFILE2):
    os.unlink(OUTFILE2)
  if os.path.exists(FINISHFILE):
    os.unlink(FINISHFILE)
  if os.path.exists(SHOTFILE):
    os.unlink(SHOTFILE)

  prx_path = TEST_ROOT + test + ".prx"

  if not os.path.exists(prx_path):
    print "You must compile the test into a PRX first (" + prx_path + ")"
    return False

  # Maybe we should start usbhostfs_pc for them?
  if not hostfs_is_ready():
    start_hostfs()
    success = wait_until(hostfs_is_ready, RECONNECT_TIMEOUT, 0.2)
    if not success:
      print "Make sure you've installed and run %s" % (HOSTFS)
      return False

  # Wait for the PSP to reconnect after a previous test.
  if not pspsh_is_ready():
    print "Waiting for PSP to connect..."

    success = wait_until(pspsh_is_ready, RECONNECT_TIMEOUT, 0.2)

    # No good, it never came back.
    if not success:
      print "Please make sure the PSP is connected"
      print "On Windows, the usb driver must be installed"
      return False

  # Okay, time to run the command.
  c = Command([PSPSH, "-p", str(PORT), "-e", prx_path + " " + " ".join(args)])
  c.run(TIMEOUT)
  if not re.match(r"^Load/Start [^ ]+ UID: 0x[0-9A-F]+ Name: TESTMODULE\s*$", c.output):
    print c.output

  # PSPSH returns right away, though, so do the timeout here.
  wait_until(lambda: os.path.exists(FINISHFILE), TIMEOUT, 0.1)

  if not os.path.exists(FINISHFILE):
    print "ERROR: Test timed out after %d seconds" % (TIMEOUT)

    # Reset the test, it's probably dead.
    os.system("%s -p %i -e reset" % (PSPSH, PORT))
  elif os.path.exists(OUTFILE2) and os.path.getsize(OUTFILE2) > 0:
    print "ERROR: Script produced stderr output"
  elif os.path.exists(OUTFILE) and os.path.getsize(OUTFILE) > 0:
    return open(OUTFILE, "rt").read()
  # It's acceptable to have a graphics-only test.
  elif os.path.exists(SHOTFILE) and os.path.getsize(SHOTFILE) > 0:
    return ""
  else:
    print "ERROR: No or empty " + OUTFILE + " was written, can't write .expected"

  return False


def gen_test_expected(test, args):
  print("Running test " + test + " on the PSP...")
  prepare_test(test, args)
  result = gen_test(test, args)

  expected_path = TEST_ROOT + test + ".expected"
  if result != False:
    # Normalize line endings on windows to avoid spurious git warnings.
    if sys.platform == 'win32':
      open(expected_path, "wt").write(result)
    else:
      shutil.copyfile(OUTFILE, expected_path)
    print "Expected file written: " + expected_path

    if os.path.exists(SHOTFILE) and os.path.getsize(SHOTFILE) > 0:
      shutil.copyfile(SHOTFILE, expected_path + ".bmp")
      print "Expected screenshot written: " + expected_path + ".bmp"

    return True

  return False

def gen_test_all_versions(test, args):
  print("Running test " + test + " on the PSP...")
  prepare_test(test, args)
  standard_result = gen_test(test, args)

  diff = False
  for name in all_versions:
    sys.stdout.write("Version %s... " % (name))
    result = gen_test(test, all_versions[name] + args)

    if result != standard_result:
      print "*** %s got a different result using %s" % (test, " ".join(all_versions[name]))
      diff = True

  return diff

def main():
  init()
  tests = []
  args = []
  for arg in sys.argv[1:]:
    if arg[0] == "-":
      args.append(arg)
    else:
      tests.append(arg.replace("\\", "/"))

  if not tests:
    tests = tests_to_generate

  if "-h" in args or "--help" in args:
    print "Usage: %s [options] cpu/icache/icache rtc/rtc...\n" % (os.path.basename(sys.argv[0]))
    print "Tests should be found under %s and omit the .prx extension." % (TEST_ROOT)
    print "Automatically runs make in the test by default.\n"
    print "Options:"
    print "  -r, --rebuild         run make rebuild for each test"
    print "  -k, --keep            do not run make before tests"
    print "      --sdkver=VER      use sceKernelSetCompiledSdkVersion(VER)"
    print "      --sdkver-func=### use sceKernelSetCompiledSdkVersion###(VER)"
    print "  -a, --all-versions    run the test for all known versions"
    return

  if "-a" in args or "--all-versions" in args:
    warnings = []
    for test in tests:
	  if gen_test_all_versions(test, args):
	    warnings.append(test)

    if len(warnings):
      print "\nDifferences:"
      for test in warnings:
        print "  " + test
  else:
    for test in tests:
      gen_test_expected(test, args)

  # End usbhostfs_pc if we started it.
  stop_hostfs()

main()
