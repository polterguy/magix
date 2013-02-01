<%@ Assembly 
    Name="Magix.SampleModules" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.SampleModules.ButtonSample" %>

<link href="media/main.css" rel="stylesheet" type="text/css" />

<mux:Button 
	runat="server"
	Text="Click me ..."
	OnClick="but_Click"
    style="width:500px;height:500px;"
    CssClass="green-button"
    id="but" />
