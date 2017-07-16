using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CSharpUtils.Templates.Runtime
{
	public abstract class TemplateCode
	{
		TemplateFactory TemplateFactory;
		public delegate Task RenderDelegate(TemplateContext Context);
		Dictionary<String, RenderDelegate> Blocks = new Dictionary<string, RenderDelegate>();
		TemplateCode ChildTemplate;
		TemplateCode ParentTemplate;

		public TemplateCode(TemplateFactory TemplateFactory = null)
		{
			this.TemplateFactory = TemplateFactory;
			this.Init();
		}

		public void Init()
		{
			this.SetBlocks(this.Blocks);
		}

		public virtual void SetBlocks(Dictionary<String, RenderDelegate> Blocks)
		{
		}

		protected void SetBlock(Dictionary<String, RenderDelegate> Blocks, String BlockName, RenderDelegate Callback)
		{
			Blocks[BlockName] = Callback;
		}

		/*
		async protected virtual Task LocalRenderAsync(TemplateContext Context)
		{
		}
		*/

		/*
		public async Task RenderAsync(TemplateContext Context)
		{
		}
		*/

#if NET_4_5
		protected abstract Task LocalRenderAsync(TemplateContext Context);

		public async Task RenderAsync(TemplateContext Context)
		{
			Context.RenderingTemplate = this;

			Exception ProducedException = null;
			try
			{
				await this.LocalRenderAsync(Context);
			}
			catch (FinalizeRenderException)
			{
			}
			catch (Exception Exception)
			{
				ProducedException = Exception;
			}
			if (ProducedException != null)
			{
				await Context.Output.WriteLineAsync(ProducedException.ToString());
				//throw (ProducedException);
			}
		}
#else
		protected abstract void LocalRender(TemplateContext Context);

		public void Render(TemplateContext Context)
		{
			Context.RenderingTemplate = this;

			Exception ProducedException = null;
			try
			{
				this.LocalRender(Context);
			}
			catch (FinalizeRenderException)
			{
			}
			catch (Exception Exception)
			{
				ProducedException = Exception;
			}
			if (ProducedException != null)
			{
				Context.Output.WriteLine(ProducedException.ToString());
				//throw (ProducedException);
			}
		}
#endif

		public String RenderToString(TemplateScope Scope = null)
		{
			if (Scope == null) Scope = new TemplateScope();
			var StringWriter = new StringWriter();
#if NET_4_5
			RenderAsync(new TemplateContext(StringWriter, Scope, TemplateFactory)).Wait();
#else
			Render(new TemplateContext(StringWriter, Scope, TemplateFactory));
#endif
			return StringWriter.ToString();
		}

		protected void SetAndRenderParentTemplate(String ParentTemplateFileName, TemplateContext Context)
		{
			this.ParentTemplate = Context.TemplateFactory.GetTemplateCodeByFile(ParentTemplateFileName);
			this.ParentTemplate.ChildTemplate = this;
#if NET_4_5
			this.ParentTemplate.LocalRenderAsync(Context);
#else
			this.ParentTemplate.LocalRender(Context);
#endif

			throw (new FinalizeRenderException());
		}

#if NET_4_5
		async protected Task CallBlockAsync(String BlockName, TemplateContext Context)
		{
			await Context.RenderingTemplate.GetFirstAscendingBlock(BlockName)(Context);
		}
#else
		protected void CallBlock(String BlockName, TemplateContext Context)
		{
			Context.RenderingTemplate.GetFirstAscendingBlock(BlockName)(Context);
		}
#endif

		protected RenderDelegate GetFirstAscendingBlock(String BlockName)
		{
			if (this.Blocks.ContainsKey(BlockName))
			{
				return this.Blocks[BlockName];
			}

			if (this.ParentTemplate != null)
			{
				return this.ParentTemplate.GetFirstAscendingBlock(BlockName);
			}
			
			throw(new Exception(String.Format("Can't find ascending parent block '{0}'", BlockName)));
		}

		protected void CallParentBlock(String BlockName, TemplateContext Context)
		{
			this.ParentTemplate.GetFirstAscendingBlock(BlockName)(Context);
		}

		public delegate void EmptyDelegate();

		protected void Autoescape(TemplateContext Context, dynamic Expression, EmptyDelegate Block)
		{
			bool OldAutoescape = Context.Autoescape;
			Context.Autoescape = Expression;
			{
				Block();
			}
			Context.Autoescape = OldAutoescape;
		}

		protected void Foreach(TemplateContext Context, String VarName, dynamic Expression, EmptyDelegate Iteration, EmptyDelegate Else = null)
		{
			int Index = 0;
			foreach (var Item in DynamicUtils.ConvertToIEnumerable(Expression))
			{
				Context.SetVar("loop", new Dictionary<String, dynamic> {
					{ "index", Index + 1 },
					{ "index0", Index },
				});
				Context.SetVar(VarName, Item);
				Iteration();
				Index++;
			}

			if (Index == 0)
			{
				if (Else != null) Else();
			}
		}
	}
}
