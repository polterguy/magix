<%@ Assembly 
    Name="Magix.modules" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.modules.PlainMenu" %>

<mux:Panel
	runat="server"
	id="wrp">
	<asp:Repeater
		runat="server"
		id="rep">
		<ItemTemplate>
			<mux:LinkButton
				runat="server"
				CssClass="span3 btn btn-block"
				Text='<%#Eval("Value")%>' />
		</ItemTemplate>
	</asp:Repeater>
</mux:Panel>
