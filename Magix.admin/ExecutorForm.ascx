<%@ Assembly 
    Name="Magix.admin" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.admin.ExecutorForm" %>

<div class="span-24 last prepend-top">
	<h1 class="span-10 prepend-7">Active Event Executor</h1>
	<div class="span-22 prepend-2">
		<mux:TextArea
			runat="server"
			id="txtIn"
			PlaceHolder="Input Nodes ..."
			CssClass="span-20 height-12 code-window" />
	</div>
	<div class="span-17 prepend-7 prepend-top">
		<mux:TextArea
			runat="server"
			PlaceHolder="Active Event Code ..."
			CssClass="span-10 height-5 code-window"
			id="activeEvent" />
		<mux:Button
			runat="server"
			id="run"
			Text="Run"
			CssClass="span-4 height-2"
			Tooltip="Runs the Active Event with the JSON Serialized content from Left Text Area"
			OnClick="run_Click" />
	</div>
	<div class="span-22 prepend-2 prepend-top">
		<mux:TextArea
			runat="server"
			id="txtOut"
			PlaceHolder="Output Nodes ..."
			CssClass="span-20 height-12 code-window" />
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
    			CssClass='<%#GetCSS(Eval("[CSS].Value")) %>'
    			ToolTip='<%# Eval("[ToolTip].Value") %>'
    			OnClick="EventClicked"
    			Text='<%# Eval("Value") %>' />
    	</ItemTemplate>
   	</asp:Repeater>
</mux:Panel>
