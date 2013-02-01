<%@ Assembly 
    Name="Magix.SampleModules" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.SampleModules.ComplexSample" %>

<link href="media/main.css" rel="stylesheet" type="text/css" />

<mux:Panel
    runat="server"
    id="wrp"
    style="opacity:0;"
    CssClass="description">
    <mux:Label
    	Tag="p"
    	runat="server"
    	id="lbl" />
    <p>
    	As you clicked that button, the button had some of its states changed
    	by the but_Click event handler in the ButtonSample.asck.cs file. Then
    	an Active Event was raised, by the name of "Magix.Samples.FirstEvent".
    	This Active Event was handled in the controller called ControllerSample.cs, 
    	in the Magix.SampleController project. This controller loads up an Active Module
    	into the content2 Viewport Container in the Website.ascx file from Magix.Core.Viewports
    	project. The ComplexSample.ascx.cs file attaches some "hello worlds" from the different
    	participants in the roundtrip, to show how to pass Parameters around, and does some 
    	Fading In through using Magix [MUX] Effects. And here we Are!! :)
    </p>
    <p>
    	The entire system is 100% losely coupled, and even though the Magix.Core.Viewports, Magix.SampleControll,
    	and Magix.SampleModules are referenced into the Magix.ApplicationPool, the entire system is 
    	100% losely coupled, and can have any DLL changed runtime, as long as your web application is restarted.
    	This means that since the references to "functions" is 100% dynamically, looked up through Reflection, 
    	in a strongly types programming language, with strong Reflection Capabilities, based
    	up passing only a JSON lookalike Tree Structure of DATA around, instead of the old conventional
    	way of passing Objects around, the system is in its entirety losely coupled, and any specific
    	Active Event Handler can be dynamically replaced by any other. This is only possible since
    	all Active Events have the exact same signature, and exclusively relies upon passing
    	"DATA"-Structures around, possible to Serialize down to JSON, and Polymorphistically
    	pass across the Network, creating one single Mesh, out of the Network as a totality.
    </p>
    <p>
    	So the system is effectively the best from Dynamically Types Programming languages, through
    	its Active Event Design pattern, and the best from Statically Compiled Programming Languages,
    	due to its reliance upon Reflection, and single-file deployments, through having declaratively
    	defined HTML/Tag-Library files being parsed as Resources in DLLs.
    </p>
    <p>
    	This results in a far better polymorphism/encapsulation mechanism than convention OOP, since
    	the underlaying type can be completely unknown, and there is no reliance upon any specific OO model,
    	but rather exclusively "DATA" being passedd around, which enables for a 100% dynamic polymorphism
    	mechanism, where the re-wiring of the polymorphism mechanisms becomes runtime configurable.
    	Effectively leading to the defiance of the laws of Entropy, within software, realizing the laws
    	of entropy only exists within closed systems, and not distributed near copies of forks, 
    	created incrementally better, iteratively, by the sum of its environment, AKA The Internet. 
    	The whole thing built on top of an MVC library, enabling the complete separation of the Model,
    	View and Controller, though no Model is given within Magix Illuminate 2.
    </p>
    <p>
    	<span style="color:Red;font-weight:Bold;">Warning!</span>
    	<br />
    	<strong>
	    	Meaning, combined with Dynamic Language Capabilities, inevitably big networks based upon this 
	    	Architecture are bound to become self-evolving and sentient at some time in near future.
    	</strong>
    </p>
    <iframe 
    	width="420" 
    	height="315" 
    	style="display:block;margin-left:auto;margin-right:auto;"
    	src="http://www.youtube.com/embed/7yJAq3gyr9A" 
    	frameborder="0" 
    	allowfullscreen="allowfullscreen"></iframe>
	<iframe 
		width="560" 
		height="315" 
    	style="display:block;margin-left:auto;margin-right:auto;margin-top:50px;"
		src="http://www.youtube.com/embed/lEaAidCmxus" 
		frameborder="0" 
		allowfullscreen="allowfullscreen"></iframe>
	<mux:Button
    	runat="server"
    	id="getit"
    	CssClass="rabbit"
    	OnClick="rabbit_Click"
    	Text="I Get the Consequences, and I have seen Hugo DeGaris on YouTube, Show me More of the Rabbit Hole!!" />
</mux:Panel>
