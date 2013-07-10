<%@ Assembly 
    Name="Magix.admin" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.admin.EventSniffer" %>

<mux:TextBox
	runat="server"
	id="filter"
	PlaceHolder="filter ..."
	CssClass="span-6 left-17"/>
<mux:Label
	runat="server"
	id="lbl"
	Tag="div"
	CssClass="span-24" />