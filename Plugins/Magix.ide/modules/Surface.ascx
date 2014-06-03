<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.ide.modules.Surface" %>

<mux:Label
    runat="server"
    id="lbl"
    Class="wysiwyg-design-surface-label left-9"
    Tag="label"
    Value="wysiwyg design surface" />

<mux:Panel
	runat="server"
	Class="wysiwyg-design-surface"
	OnClick="wrp_Click"
	id="wrp" />
