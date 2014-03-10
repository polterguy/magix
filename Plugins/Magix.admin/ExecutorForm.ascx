<%@ Assembly 
    Name="Magix.admin" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.admin.ExecutorForm" %>

<div class="fill-width relative">
    <mux:LinkButton
    	runat="server"
    	CssClass="indent-event-executor"
    	id="indent"
    	OnClick="indent_Click" />
    <mux:LinkButton
    	runat="server"
    	CssClass="de-indent-event-executor"
    	id="deindent"
    	OnClick="deindent_Click" />
	<mux:TextArea
		runat="server"
		id="txtIn"
		Rows="15"
		PlaceHolder="input nodes ..."
		CssClass="fill-width monospaced" />
	<mux:Panel
		runat="server"
		ID="exeWrp"
		DefaultWidget="run"
		CssClass="btn-group span-17 last">
		<mux:TextBox
			id="activeEvent"
			runat="server"
			PlaceHolder="active event ..."
			CssClass="span-11 monospaced input-large" />
		<mux:Button
			runat="server"
			id="run"
			Text="execute"
			AccessKey="X"
			CssClass="span-3 btn-large"
			OnClick="run_Click" />
		<mux:Button
			runat="server"
			id="move"
			Text="move up"
			AccessKey="U"
			CssClass="span-3 last btn-large"
			OnClick="move_Click" />
	</mux:Panel>
	<mux:TextArea
		runat="server"
		id="txtOut"
		Rows="15"
		PlaceHolder="output nodes ..."
		CssClass="fill-width" />
</div>