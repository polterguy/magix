/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /**
     * A single-line type of 'give me some text input' type of widget. This widget
     * is a wrapper around the input type="text" type of widget. If you need 
     * multiple lines of input, you should rather use the TextArea widget. However 
     * this widget is useful for cases when you need the user to give you one line 
     * of text input. See also the RichEdit widget if you need rich formatting 
     * of your text. This widget can also be set to 'password mode', which means
     * whatever is typed into the widget will not be visible on the screen. Please
     * notice though that by default, this will be transfered to the server in an
     * unsecure manner, so this is only a mechanism to make sure that other people
     * cannot read over your shoulder to see what you're 'secretly' trying to type
     * into your TextBox. Use SSL or other types of security to actually implement
     * safe transmitting of your passwords and similar 'secret text strings'.
     */
    public class TextBox : BaseWebControlFormElementInputText
    {
        /**
         * The mode your textbox is set to. The default value of this property
         * is Normal which means the characters will display as normal. You can set
         * the mode to Password, which will not show the characters as they're written
         * which may be useful for password text boxes.
         */
        public enum TextBoxMode
        {
            /**
             * Normal mode TextBox.
             */
            Normal,

            /**
             * Telephone mode TextBox, will validate accordingly to HTML5 tel type of 
             * input element.
             */
            Phone,

            /**
             * Search
             */
            Search,

            /**
             * Uniform Resource Locator, URL
             */
            Url,

            /**
             * Email
             */
            Email,

            /**
             * Date and Time
             */
            DateTime,

            /**
             * Date
             */
            Date,

            /**
             * Month
             */
            Month,

            /**
             * Week
             */
            Week,

            /**
             * Time
             */
            Time,

            /**
             * Local Date and Time
             */
            DateTimeLocal,

            /**
             * Decimal number
             */
            Number,

            /**
             * Range, inbetween two values
             */
            Range,

            /**
             * Color type of input
             */
            Color,

            /**
             * Password mode TextBox, will not show the characters as they're written.
             */
            Password
        };

        /**
         * If this event is subscribed to, you will get notified when the carriage return, 
         * enter or return key is pressed and released. Useful for being able to give 'simple
         * data' without having to physically click some 'save button' to submit it.
         */
        public event EventHandler EnterPressed;

        /**
         * The mode your textbox is set to. The default value of this property
         * is Normal which means the characters will display as normal. You can set
         * the mode to Password, which will not show the characters as they're written
         * which may be useful for password text boxes.
         */
        public TextBoxMode TextMode
        {
            get { return ViewState["TextMode"] == null ? TextBoxMode.Normal : (TextBoxMode)ViewState["TextMode"]; }
            set
            {
                if (value != TextMode)
                    SetJsonValue("Type", value == TextBoxMode.Password ? "text" : "password");
                ViewState["TextMode"] = value;
            }
        }

        /**
         * Ghost text displayed only if there is no value in the textbox. Useful for giving 
         * end user hints and clues about what type of field this is.
         */
        public string PlaceHolder
        {
            get { return ViewState["PlaceHolder"] == null ? "" : (string)ViewState["PlaceHolder"]; }
            set
            {
                if (value != PlaceHolder)
                    SetJsonGeneric("placeholder", value);
                ViewState["PlaceHolder"] = value;
            }
        }

        /**
         * If true, will automatically Capitalize the first letter of the first word in 
         * the textbox, if supported by the browser
         */
        public bool AutoCapitalize
        {
            get { return ViewState["AutoCapitalize"] == null ? true : (bool)ViewState["AutoCapitalize"]; }
            set
            {
                if (value != AutoCapitalize)
                    SetJsonGeneric("autocapitalize", value ? "off" : "on");
                ViewState["AutoCapitalize"] = value;
            }
        }

        /**
         * If true, which is the default value, the browser will automatically correct 
         * spelling errors and such, which can be very annoying sometimes ...
         */
        public bool AutoCorrect
        {
            get { return ViewState["AutoCorrect"] == null ? true : (bool)ViewState["AutoCorrect"]; }
            set
            {
                if (value != AutoCorrect)
                    SetJsonGeneric("autocorrect", value ? "off" : "on");
                ViewState["AutoCorrect"] = value;
            }
        }

        /**
         * If true, the default, it will try to automatically complete the form field 
         * according to your browser's settings for names and such
         */
        public bool AutoComplete
        {
            get { return ViewState["AutoComplete"] == null ? true : (bool)ViewState["AutoComplete"]; }
            set
            {
                if (value != AutoComplete)
                    SetJsonGeneric("autocomplete", value ? "on" : "off");
                ViewState["AutoComplete"] = value;
            }
        }

        /**
         * The maximum number of characters this TextBox allows the user to type in.
         */
        public int MaxLength
        {
            get { return ViewState["MaxLength"] == null ? 0 : (int)ViewState["MaxLength"]; }
            set 
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "The MaxLength property can not have a negative value.");
                else if (value != MaxLength)
                    SetJsonGeneric("maxLength", value.ToString());
                ViewState["MaxLength"] = value;
            }
        }

        public override void RaiseEvent(string name)
        {
            switch (name)
            {
                case "enter":
                    if (EnterPressed != null)
                        EnterPressed(this, new EventArgs());
                    break;
                default:
                    base.RaiseEvent(name);
                    break;
            }
        }

        protected override string GetEventsRegisterScript()
        {
            string evts = base.GetEventsRegisterScript();
            if (EnterPressed != null)
            {
                if (evts.Length != 0)
                    evts += ",";
                evts += "['enter']";
            }
			return evts;
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("input"))
            {
                AddAttributes(el);
            }
        }

        protected override void AddAttributes(Element el)
        {
            switch (TextMode)
            {
                case TextBoxMode.Normal:
                    el.AddAttribute("type", "text");
                    break;
                case TextBoxMode.Password:
                    el.AddAttribute("type", "password");
                    break;
                case TextBoxMode.Email:
                    el.AddAttribute("type", "email");
                    break;
                case TextBoxMode.Phone:
                    el.AddAttribute("type", "tel");
                    break;
                case TextBoxMode.Number:
                    el.AddAttribute("type", "number");
                    break;
            }
            if (MaxLength > 0)
                el.AddAttribute("maxlength", MaxLength.ToString());
            if (!AutoCapitalize)
                el.AddAttribute("autocapitalize", "off");
            if (AutoComplete)
                el.AddAttribute("autocomplete", "on");
            if (!AutoCorrect)
                el.AddAttribute("autocorrect", "off");
            if (!string.IsNullOrEmpty(PlaceHolder))
                el.AddAttribute("placeholder", PlaceHolder);
            base.AddAttributes(el);
        }
    }
}
