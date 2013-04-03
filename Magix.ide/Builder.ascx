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

<mux:Button
	runat="server"
	id="run"
	Text="execute"
	AccessKey="X"
	CssClass="span2 btn btn-primary"
	OnClick="run_Click" />
