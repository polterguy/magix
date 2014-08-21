<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    EnableViewState="true"
    AutoEventWireup="true" 
    Inherits="Magix.ide.modules.ConsoleLogger" %>

<mux:Label
	runat="server"
	id="lbl" 
	Tag="div"
	Class="fill-width column last console-logger" />
