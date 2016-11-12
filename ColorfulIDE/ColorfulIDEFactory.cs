using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;

namespace HiTeam.ColorfulIDE
{
	#region Adornment Factory
	/// <summary>
	/// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
	/// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
	/// </summary>
	[Export(typeof(IWpfTextViewCreationListener))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	internal sealed class ColorfulIdeAdornmentFactory : IWpfTextViewCreationListener
	{
		[Import(typeof(SVsServiceProvider))]
		internal System.IServiceProvider ServiceProvider { get; set; }

        [Import]
        internal IEditorFormatMapService FormatMapService;
		
		/// <summary>
		/// Defines the adornment layer for the scarlet adornment. This layer is ordered 
		/// after the selection layer in the Z-order
		/// </summary>
		[Export(typeof(AdornmentLayerDefinition))]
		[Name("Colorful-IDE")]
		[Order(After = PredefinedAdornmentLayers.DifferenceChanges)]
		public AdornmentLayerDefinition EditorAdornmentLayer { get; set; }

		/// <summary>
		/// Instantiates a MyIDE manager when a textView is created.
		/// </summary>
		/// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
		public void TextViewCreated(IWpfTextView textView)
		{
		    new ColorfulIde(textView, ServiceProvider);
		}
	}
	
	#endregion //Adornment Factory
}
