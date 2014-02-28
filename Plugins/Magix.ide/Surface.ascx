<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.ide.Surface" %>

<mux:Panel
	runat="server"
	ToolTip="design surface for your controls"
	CssClass="wysiwyg-design-surface"
	OnClick="wrp_Click"
	id="wrp" />
