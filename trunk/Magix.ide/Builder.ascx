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

<hr class="span11" />

<mux:Panel
	runat="server"
	CssClass="span12"
	style="display:none;"
	id="wrp2">

	<mux:TextArea
		runat="server"
		id="console"
		Rows="6"
		PlaceHolder="console ..."
		CssClass="span12 input-block-level monospaced" />

	<div class="input-append offset5 span7">
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
		Rows="6"
		PlaceHolder="output ..."
		CssClass="span12 input-block-level monospaced" />

</mux:Panel>

<mux:Button
	runat="server"
	id="hider"
	CssClass="btn btn-primary span2"
	OnClick="hider_Click"
	Text="show" />