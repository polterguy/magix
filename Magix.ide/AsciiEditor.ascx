<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.ide.AsciiEditor" %>

<mux:TextArea
	runat="server"
	CssClass="fill-width monospaced ascii-editor"
	id="surface" />
<mux:Button
	runat="server"
	id="save"
	CssClass="btn-large span-3 right last"
	Text="save"
	OnClick="save_Click" />


