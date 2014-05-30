<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.ide.modules.Surface" %>

<mux:Panel
	runat="server"
	Title="design surface for your controls"
	Class="wysiwyg-design-surface"
	OnClick="wrp_Click"
	id="wrp" />
