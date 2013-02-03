<%@ Assembly 
    Name="Magix.SampleModules" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.SampleModules.NodeEditor" %>

<link href="media/main.css" rel="stylesheet" type="text/css" />

<div class="obscurer">
</div>
<mux:Button
	runat="server"
	id="close"
	Text="Close"
	OnClick="close_Click"
	CssClass="button-close" />
<mux:Button
	runat="server"
	id="closeSave"
	Text="Save"
	OnClick="closeSave_Click"
	CssClass="button-close-save" />
<mux:Panel
	runat="server"
	CssClass="node-wrapper"
	id="wrp">
	<mux:TextArea
		runat="server"
		id="txt" />
</mux:Panel>
