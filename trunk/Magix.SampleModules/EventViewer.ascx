<%@ Assembly 
    Name="Magix.SampleModules" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.SampleModules.EventViewer" %>

<link href="media/main.css" rel="stylesheet" type="text/css" />

<div class="nodes-wrapper">
	<p class="active-event-label">
		Active Event Name
	</p>
	<mux:TextArea
		runat="server"
		PlaceHolder="Active Event Name ..."
		CssClass="active-event-name"
		id="activeEvent" />
	<mux:TextArea
		runat="server"
		id="txtIn"
		PlaceHolder="Input Nodes ..."
		CssClass="nodes" />
	<mux:Button
		runat="server"
		id="run"
		Text="&gt;&gt;"
		CssClass="run-button"
		Tooltip="Runs the Active Event with the JSON Serialized content from Left Text Area"
		OnClick="run_Click" />
	<mux:Button
		runat="server"
		id="paste"
		Text="&lt;&lt;"
		CssClass="paste-button"
		Tooltip="Paste the content from the Output Text Area into the Input Text Area"
		OnClick="paste_Click" />
	<mux:TextArea
		runat="server"
		id="txtOut"
		PlaceHolder="Output Nodes ..."
		CssClass="nodes" />
</div>

<mux:Panel
    runat="server"
    CssClass="repeater-wrapper"
    id="wrp">
    <asp:Repeater
    	runat="server"
    	id="rep">
    	<ItemTemplate>
    		<mux:LinkButton
    			runat="server"
    			CssClass="active-event"
    			OnClick="EventClicked"
    			Text='<%# Eval("Value") %>' />
    	</ItemTemplate>
   	</asp:Repeater>
</mux:Panel>
<br style="clear:both;" />