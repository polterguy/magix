<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.ide.Builder" %>

<mux:Panel
	runat="server"
	CssClass="span12"
	id="wrp" />

<mux:TextArea
	runat="server"
	id="console"
	Rows="12"
	PlaceHolder="command ..."
	CssClass="span12 input-block-level monospaced" />

<div class="input-append span9">
	<mux:Button
		runat="server"
		id="run"
		Text="execute"
		AccessKey="X"
		CssClass="span2 btn btn-primary"
		OnClick="run_Click" />
</div>

<mux:TextArea
	runat="server"
	id="output"
	Rows="12"
	PlaceHolder="output ..."
	CssClass="span12 input-block-level monospaced" />
