using AjaxControlToolkit.Design;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AjaxControlToolkit {

    /// <summary>
    /// PopupControl is an ASP.NET AJAX extender that can be attached to any control in order to open
    /// a popup window that displays additional content. This popup window will probably be interactive
    /// and will probably be within an ASP.NET AJAX UpdatePanel, so it will be able to perform complex
    /// server-based processing (including postbacks) without affecting the rest of the page. The popup
    /// window can contain any content, including ASP.NET server controls, HTML elements, etc. Once the
    /// work of the popup window is done, a simple server-side call dismisses it and triggers any relevant
    /// script on the client to run and update the page dynamically. 
    /// </summary>
    [ClientScriptResource("Sys.Extended.UI.PopupControlBehavior", Constants.PopupControlName)]
    [RequiredScript(typeof(PopupExtender))]
    [RequiredScript(typeof(CommonToolkitScripts))]
    [TargetControlType(typeof(WebControl))]
    [Designer(typeof(PopupControlExtenderDesigner))]
    [ToolboxBitmap(typeof(ToolboxIcons.Accessor), Constants.PopupControlName + Constants.IconPostfix)]
    public class PopupControlExtender : DynamicPopulateExtenderControlBase {
        bool _shouldClose;
        string _closeString;
        Page _proxyForCurrentPopup;
        EventHandler _pagePreRenderHandler;

        public PopupControlExtender() {
        }

        // Constructor with a page - Used to initialize proxy controls only.
        private PopupControlExtender(Page page) {
            _proxyForCurrentPopup = page;

            // In this case, the control acts as a proxy
            // It is not actually added to control collections and does not participate in the page cycle
            // Attach the Page.PreRender event
            _pagePreRenderHandler = new EventHandler(Page_PreRender);
            _proxyForCurrentPopup.PreRender += _pagePreRenderHandler;
        }

        /// <summary>
        /// Gets a proxy PopupControlExtender representing the currently active popup on the specified page
        /// </summary>
        /// <remarks>
        /// Only the Cancel and Commit methods should be called on the proxy
        /// </remarks>
        /// <param name="page">Page</param>
        /// <returns>PopupControlExtender</returns>
        public static PopupControlExtender GetProxyForCurrentPopup(Page page) {
            var popupControlExtender = new PopupControlExtender(page);
            return popupControlExtender;
        }

        /// <summary>
        /// Cancels the popup control and hides it, abandoning any results it has.
        /// </summary>
        public void Cancel() {
            // It is possible for Cancel() to be called numerous times during the same postback so we just remember the desired state
            // Pass the magic cancel string as the result
            _closeString = "$$CANCEL$$";
            _shouldClose = true;
        }

        /// <summary>
        /// Commits the popup control and hides it, applying the specified result.
        /// </summary>
        /// <param name="result">Result</param>
        public void Commit(string result) {
            // It is possible for Commit() to be called numerous times during the same postback so we just remember the desired state
            _closeString = result;
            _shouldClose = true;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            // Attach the Page.PreRender event - Make sure we hook up only once
            if(_pagePreRenderHandler == null) {
                _pagePreRenderHandler = new EventHandler(Page_PreRender);
                Page.PreRender += _pagePreRenderHandler;
            }
        }

        protected void Page_PreRender(object sender, EventArgs e) {
            // If Cancel()/Commit() were called, close the popup now
            if(_shouldClose)
                Close(_closeString);
        }

        // Closes the popup control, applying the specified result or abandoning it
        void Close(string result) {
            if(_proxyForCurrentPopup == null) {
                // Normal call - Simply register the relevant data item for the TargetControl
                ScriptManager.GetCurrent(Page).RegisterDataItem(TargetControl, result);
            } else {
                // Proxy call - Add a LiteralControl to pass the information down to the interested PopupControlExtender
                var literalControl = new LiteralControl();
                literalControl.ID = "_PopupControl_Proxy_ID_";
                _proxyForCurrentPopup.Controls.Add(literalControl);
                ScriptManager.GetCurrent(_proxyForCurrentPopup).RegisterDataItem(literalControl, result);
            }
        }

        /// <summary>
        /// The ID of the extender control.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [ClientPropertyName("extenderControlID")]
        public string ExtenderControlID {
            get { return GetPropertyValue("ExtenderControlID", String.Empty); }
            set { SetPropertyValue("ExtenderControlID", value); }
        }

        /// <summary>
        /// The ID of the control to display.
        /// </summary>
        [ExtenderControlProperty]
        [IDReferenceProperty(typeof(WebControl))]
        [RequiredProperty]
        [DefaultValue("")]
        [ClientPropertyName("popupControlID")]
        public string PopupControlID {
            get { return GetPropertyValue("PopupControlID", String.Empty); }
            set { SetPropertyValue("PopupControlID", value); }
        }

        // Optional name of the target control's property to set with the result of the popup
        /// <summary>
        /// Optional setting specifying the property on the control being extended
        /// that should be set with the result of the popup.
        /// </summary>
        /// <remarks>
        /// If not present, the default "value" property will be used.
        /// </remarks>
        [ExtenderControlProperty]
        [DefaultValue("")]
        [ClientPropertyName("commitProperty")]
        public string CommitProperty {
            get { return GetPropertyValue("CommitProperty", String.Empty); }
            set { SetPropertyValue("CommitProperty", value); }
        }

        // Optional script to run after setting the target control's property
        /// <summary>
        /// Optional setting specifying additional script to run after setting the result of the popup.
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        [ClientPropertyName("commitScript")]
        public string CommitScript {
            get { return GetPropertyValue("CommitScript", String.Empty); }
            set { SetPropertyValue("CommitScript", value); }
        }

        // Optional position for the popup window (relative to the target control)
        /// <summary>
        /// Optional setting specifying where the popup should be positioned relative to the target control.
        /// </summary>
        /// <remarks>
        /// Can be Left, Right, Top, Bottom or Center.
        /// </remarks>
        [ExtenderControlProperty]
        [DefaultValue(PopupControlPopupPosition.Center)]
        [ClientPropertyName("position")]
        public PopupControlPopupPosition Position {
            get { return GetPropertyValue("Position", PopupControlPopupPosition.Center); }
            set { SetPropertyValue("Position", value); }
        }

        // Optional X (horizontal) offset for the popup window (relative to the target control)
        /// <summary>
        /// The number of pixels to offset the Popup from its default position, as specified by Position.
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue(0)]
        [ClientPropertyName("offsetX")]
        public int OffsetX {
            get { return GetPropertyValue("OffsetX", 0); }
            set { SetPropertyValue("OffsetX", value); }
        }

        // Optional Y (vertical) offset for the popup window (relative to the target control)
        /// <summary>
        /// The number of pixels to offset the Popup from its default position, as specified by Position.
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue(0)]
        [ClientPropertyName("offsetY")]
        public int OffsetY {
            get { return GetPropertyValue("OffsetY", 0); }
            set { SetPropertyValue("OffsetY", value); }
        }

        /// <summary>
        /// The OnShow animation will be played each time the popup is displayed.
        /// The popup will be positioned correctly but hidden. The animation can use
        /// to display the popup along with any other visual effects.
        /// </summary>
        [ExtenderControlProperty]
        [ClientPropertyName("onShow")]
        [Browsable(false)]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Animation OnShow {
            get { return GetAnimation(ref _onShow, "OnShow"); }
            set { SetAnimation(ref _onShow, "OnShow", value); }
        }
        Animation _onShow;

        /// <summary>
        /// The OnHide animation will be played each time the popup is hidden.
        /// </summary>
        [ExtenderControlProperty]
        [ClientPropertyName("onHide")]
        [Browsable(false)]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Animation OnHide {
            get { return GetAnimation(ref _onHide, "OnHide"); }
            set { SetAnimation(ref _onHide, "OnHide", value); }
        }
        Animation _onHide;

        // Convert server IDs into ClientIDs for animations
        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ResolveControlIDs(_onShow);
            ResolveControlIDs(_onHide);
        }
    }

}