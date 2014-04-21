<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.ide.AsciiEditor" %>

<mux:Panel
	runat="server"
	id="wrp">
	<label class="fill-width">file name</label>
	<mux:TextBox
		runat="server"
		id="path"
		CssClass="fill-width"/>
	<label class="fill-width top-1">file content</label>
	<mux:TextArea
		runat="server"
		CssClass="fill-width monospaced ascii-editor"
		id="surface" />
	<mux:Button
		runat="server"
		id="delete"
		CssClass="btn-large span-3 right last"
		Text="delete"
		OnClick="delete_Click" />
	<mux:Button
		runat="server"
		id="save"
		CssClass="btn-large span-3 right"
		Text="save"
		OnClick="save_Click" />
	<mux:Button
		runat="server"
		id="close"
		CssClass="btn-large span-3 right"
		Text="close"
		OnClick="close_Click" />
</mux:Panel>

