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
		Class="fill-width"/>
	<label class="fill-width top-1">file content</label>
	<mux:TextArea
		runat="server"
		Class="fill-width monospaced ascii-editor"
		id="surface" />
	<mux:Button
		runat="server"
		id="Button1"
		Class="btn-large span-3 right last"
		Value="delete"
		OnClick="delete_Click" />
	<mux:Button
		runat="server"
		id="preview"
		Class="btn-large span-3 right"
        Visible="false"
		Value="preview"
		OnClick="preview_Click" />
	<mux:Button
		runat="server"
		id="save"
		Class="btn-large span-3 right"
		Value="save"
		OnClick="save_Click" />
	<mux:Button
		runat="server"
		id="close"
		Class="btn-large span-3 right"
		Value="close"
		OnClick="close_Click" />
</mux:Panel>

