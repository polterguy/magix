<%@ Assembly 
    Name="Magix.SampleModules" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.SampleModules.EventViewer" %>

<div class="span-24 last prepend-top">
	<h1 class="span-10 prepend-7">Active Event Executor</h1>
	<div class="span-24 prepend-2">
		<mux:TextArea
			runat="server"
			id="txtIn"
			PlaceHolder="Input Nodes ..."
			CssClass="span-20 height-12" />
	</div>
	<div class="span-24 prepend-7 prepend-top">
		<mux:TextArea
			runat="server"
			PlaceHolder="Active Event Code ..."
			CssClass="span-10 height-5"
			id="activeEvent" />
		<mux:Button
			runat="server"
			id="run"
			Text="Run"
			CssClass="span-4"
			Tooltip="Runs the Active Event with the JSON Serialized content from Left Text Area"
			OnClick="run_Click" />
	</div>
	<div class="span-24 prepend-2 prepend-top">
		<mux:TextArea
			runat="server"
			id="txtOut"
			PlaceHolder="Output Nodes ..."
			CssClass="span-20 height-12" />
	</div>
</div>

<mux:Panel
    runat="server"
    CssClass="span-24 last prepend-top"
    id="wrp">
    <asp:Repeater
    	runat="server"
    	id="rep">
    	<ItemTemplate>
    		<mux:LinkButton
    			runat="server"
    			CssClass="span-7"
    			OnClick="EventClicked"
    			Text='<%# Eval("Value") %>' />
    	</ItemTemplate>
   	</asp:Repeater>
</mux:Panel>
