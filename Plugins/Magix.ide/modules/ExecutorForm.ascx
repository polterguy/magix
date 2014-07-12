<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.ide.modules.ExecutorForm" %>

<div class="fill-width relative last">
    <mux:LinkButton
    	runat="server"
    	Class="indent-event-executor"
    	id="indent"
    	OnClick="indent_Click" />
    <mux:LinkButton
    	runat="server"
    	Class="de-indent-event-executor"
    	id="deindent"
    	OnClick="deindent_Click" />
	<mux:TextArea
		runat="server"
		id="txtIn"
		Rows="15"
		PlaceHolder="input nodes ..."
		Class="fill-width monospaced hyperlisp-executor"
        OnTextChanged="txtIn_TextChanged" />
</div>
<mux:Panel
	runat="server"
	ID="exeWrp"
	Default="run"
	Class="btn-group fill-width last column">
	<mux:TextBox
		id="activeEvent"
		runat="server"
		PlaceHolder="active event ..."
		Class="span-11 monospaced large" />
	<mux:Button
		runat="server"
		id="run"
		Value="execute"
		AccessKey="X"
		Class="span-3 large"
		OnClick="run_Click" />
	<mux:Button
		runat="server"
		id="move"
		Value="move up"
		AccessKey="U"
		Class="span-3 last large"
		OnClick="move_Click" />
</mux:Panel>
<mux:TextArea
	runat="server"
	id="txtOut"
	Rows="15"
	PlaceHolder="output nodes ..."
    Class="fill-width monospaced hyperlisp-executor-result" />
