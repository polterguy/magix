<%@ Assembly 
    Name="Magix.admin" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.admin.EventSniffer" %>

<h3>stack trace of all hyper lisp code</h3>

<mux:Label
	runat="server"
	id="lbl"
	Tag="div"
	CssClass="span-22 last" />
