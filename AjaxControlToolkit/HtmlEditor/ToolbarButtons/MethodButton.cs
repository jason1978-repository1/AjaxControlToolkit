#pragma warning disable 1591
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI;

namespace AjaxControlToolkit.HtmlEditor.ToolbarButtons {

    [RequiredScript(typeof(CommonToolkitScripts))]
    [ClientScriptResource("Sys.Extended.UI.HtmlEditor.ToolbarButtons.MethodButton", Constants.HtmlEditorMethodButtonName)]
    public class MethodButton : DesignModeImageButton {
    }

}

#pragma warning restore 1591