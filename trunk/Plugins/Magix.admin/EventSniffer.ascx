<%@ Assembly 
    Name="Magix.admin" %>

<%@ Control 
    Language="C#" 
    EnableViewState="false"
    AutoEventWireup="true" 
    Inherits="Magix.admin.EventSniffer" %>

<h3 class="span-22 last bottom-1 center-text">stack trace of all hyper lisp code</h3>

<mux:Label
	runat="server"
	id="lbl" 
	Tag="div"
	Class="span-22 last" />
