<%@ Assembly 
    Name="Magix.ide" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.ide.Editor" %>

<link rel="stylesheet" href="media/xing-wysihtml5/website/css/stylesheet.css" />

<div class="wysiwyg-editor-textarea">
	<div id="wysihtml5-editor-toolbar" style="display:none;">
		<header>
			<ul class="commands">
				<li data-wysihtml5-command="bold" title="Make text bold (CTRL + B)" class="command"></li>
				<li data-wysihtml5-command="italic" title="Make text italic (CTRL + I)" class="command"></li>
				<li data-wysihtml5-command="insertUnorderedList" title="Insert an unordered list" class="command"></li>
				<li data-wysihtml5-command="insertOrderedList" title="Insert an ordered list" class="command"></li>
				<li data-wysihtml5-command="createLink" title="Insert a link" class="command"></li>
				<li data-wysihtml5-command="insertImage" title="Insert an image" class="command"></li>
				<li data-wysihtml5-command="formatBlock" data-wysihtml5-command-value="h1" title="Insert headline 1" class="command"></li>
				<li data-wysihtml5-command="formatBlock" data-wysihtml5-command-value="h2" title="Insert headline 2" class="command"></li>
				<li data-wysihtml5-command-group="foreColor" class="fore-color" title="Color the selected text" class="command">
				<ul>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="silver"></li>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="gray"></li>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="maroon"></li>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="red"></li>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="purple"></li>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="green"></li>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="olive"></li>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="navy"></li>
					<li data-wysihtml5-command="foreColor" data-wysihtml5-command-value="blue"></li>
				</ul>
				</li>
				<li data-wysihtml5-command="insertSpeech" title="Insert speech" class="command"></li>
				<li data-wysihtml5-action="change_view" title="Show HTML" class="action"></li>
			</ul>
		</header>
		<div data-wysihtml5-dialog="createLink" style="display: none;">
			<label>
				Link:
				<input data-wysihtml5-dialog-field="href" value="http://" />
			</label>
			<a data-wysihtml5-dialog-action="save">OK</a>&nbsp;<a data-wysihtml5-dialog-action="cancel">Cancel</a>
		</div>

		<div data-wysihtml5-dialog="insertImage" style="display: none;">
			<label>
				Image:
				<input data-wysihtml5-dialog-field="src" value="http://" />
			</label>
			<a data-wysihtml5-dialog-action="save">OK</a>&nbsp;<a data-wysihtml5-dialog-action="cancel">Cancel</a>
		</div>
	</div>

	<mux:TextArea
		runat="server"
		id="surface" 
		CssClass="wysiwyg"
		style="width:810px;height:500px;"
		placeholder="Enter your text ..." />
</div>
