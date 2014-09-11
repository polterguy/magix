Magix Illuminate
=====

Magix Illuminate Phosphorus

Magix is a web application platform that allows you to create web apps for deskktop systems, iPhones, Androids, Tablets, Linux, Windows, Mac OS X, and practically anything that can run HTML and JavaScript. Its aim is to reduce the pain of development to such an extent that also non-developers can to some extend create their own web apps. Magix is Open Source licensed under the terms of the MITx11 license.

Magix is both a framework for creation of your own apps, using C# and .Net, in addition to being a fully functional platform you can install and start using immediately, serving your web needs, whatever they may be. An example of this, is that one of the sample apps that comes bundled with Magix is Mjølner Mail, which allows you to create your own "Gmail", except with the possibility of sending encrypted emails using S/MIME. Another sample application that comes bundled with Magix is a "Vanity QR code generator", that allows you to create your own QR codes, with your own texture files used for rendering your QR code, and nicer and smoother looks than the default QR codes you can create with most other QR code generators.

Magix also contains a web based Visual IDE (Integrated Development Environment) for creating your own web parts from within the browser, which you can use in your pages and applications. You can tie your business logic into your web parts by using the built-in script programming language called "Hyperlisp", which allows you to tie together functionality within your web parts with C# or VB.NET CLR code. Or you can build your own Active Events in C#, which you invoke from your web parts, using Hyperlisp or other C# parts of your project

With Magix you can either create your logic in C#, VB.NET or any other .Net programming language, or you can create your applications 100% visually, through the browser, using the built-in IDE and Hyperlisp

Magix also contains a complete Ajax library, for most apps needs, that allows you to create Ajax functionality, without having to resort to JavaScript, but exclusively working with WebControls in ASP.NET Web Pages, using a declarative syntax. These Ajax Controls can be used to create web parts, which can be consumedd in Hyperlisp. You can also create Ajax web parts, exclusively using Hyperlisp if you wish.

If this is not an option, you can even create relatively complex and rich web applications by following wizards and filling out form-data, orchestrating your web apps together 100% visually. This way you can create web apps, even if you have never had any experience with system development. If you're skilled enough to use Microsoft Excel or Word, then you're skilled enough to create simple web apps with Magix

For a thorough explanation of Magix, please check out Magix' youtube channel at; https://www.youtube.com/MareMara13

Or watch the introductory video at; https://www.youtube.com/watch?v=sRY94BoMcNY


##############################################################################################
# Active Events
##############################################################################################

Magix is created around one axiom called "Active Events". Active Events are an alternative to methods and functions for invoking functionality, and allows for loosely coupling together different modules and components, orchestrating your application together, with a much more plugable architecture

To create an Active Event you would do something like this

```
public class ControllerSample : ActiveController
{
  [ActiveEvent(Name = "my-company.my-active-event")]
  public void foo(object sender, ActiveEventArgs e)
  {
    /* extract parameters passsed in from e.Params */
    DateTime date = e.Params["my-parameter"].Get<DateTime>(DateTime.Now);

    /* do logic ... */
    date = date.AddDays(1);

    /* return values back to caller through e.Params */
    e.Params["ret-val"].Value = date;
  }
}
```

To consume the above active event, all you need to do is to create a Node passed in as parameters to your Active Event and invoke it using RaiseActiveEvent

```
Node node = new Node();
node["my-parameter"].Value = DateTime.Now;
ActiveEvents.Instance.RaiseActiveEvent(
  this,
  "my-company.my-active-event",
  node);
```

You can pass in and return as many parameters as you wish to an Active Event. Active Events can also easily be consumed from the supplied scripting language called Hyperlisp. Conceptually Active Events can be compared to signals and slots from e.g. Qt and similar frameworks


##############################################################################################
# Hyperlisp
##############################################################################################

Magix also contains its own programming language, which is a script language for tying together business logic written in C# with GUI in your browwser. Below is an example of how to create a web part in Hyperlisp that shows a button, that consumes the above C# Active Event

```
magix.forms.create-web-part
  class=>span-22 last boxed
  controls
    button=>my-button
      class=>span-5 large
      onclick
        my-company.my-active-event
        using=>magix.viewport
          show-message
            message=>@"return value from active event was {0}"
              =>[my-company.my-active-event][ret-val].Value
```

Using Hyperlisp, you can create fairly complex features. For instance, the Mjølner Mail client, and the web part IDE designer, is almost exclusively built using Hyperlisp. The beauty of Magix comes from being able to tie together dynamic functionality with C# and VB.NET code compiled to the CLR, and such create a more dynamic, flexible and agile solution as an end result. To teach yourself Hyperlisp, Magix contains a built-in Hyperlisp Executor, that allows you to create and execute Hyperlisp through your browser.

Hyperlisp is a commpletely unique programming language, which allows you to directly modify the execution tree itself. In fact, Hyperlisp is neither an interpreted language nor a compiled language. It is in fact a simple syntax which allows you to create tree structures which the runtime can accept as executable code, allowing you to create the execution tree the runtime executes directly as your code. This facilitates for a lot of really interesting traits, such as the ability to directly modify the code currently being executedd, and so on. Hyperlisp is a completely unique programming language, facilitating for features which are easy to become acquinted with and love once you've learned the basic syntax. Hyperlisp is not Lisp btw in any ways, it is only inspired by Lisp. Hyperlisp also does not compile down to IL code in any ways, but is simply a syntactic helper built on top of Active Events, allowing for dynamically created code to coexist with compiled code. Hyperlisp brings the best of the static programming languages into the world of the best of the dyamic programming languages

Magix is also a complete MVC framework, which allows you to create your applications within a much more scalable architecture than what most other frameworks do. In Magix a Controller is called an ActiveController, a View an ActiveModule and then there is no need for a Model, since the underlaying Node structure makes the Model redundant, by allowing you to serialize data directly into the databaseusing the Hyperlisp format.


##############################################################################################
# The Magix database
##############################################################################################

The combination of the integrated database, which is storing its data as Hyperlisp tree structures, Hyperlisp and ActiveEvents, makes all mapping technologies between your database and your code completely redundant. To store something into the database, simply invoke the magix.data.save active event, as the sample below shows you

```
magix.data.save
  value
    type=>your-company.customer-object
    customer
      name=>Acme, Inc.
      email=>acme@inc.com
    contacts
      =>
        name=>John Doe
        title=>CTO
      =>
        name=>Jane Doe
        title=>CEO
```

Then later when you wish to retrieve all objects of the above type, you can use something like the code below

```
magix.data.load
  prototype
    type=>your-company.customer-object
```

Which will return all customers from your database. Or if you wish to retrieve a specific customer object, you can use the "id" parameter to magix.data.load, instead of a prototype object

If you wish to retrieve a customer whose name value contains the string "Doe", you can use wildcards, like below

```
magix.data.load
  prototype
    type=>your-company.customer-object
    name=>%Doe%
```

This readme however is not the place for showing all the features of Magix, since that would require megabytes of text to be written. One resource for additional learning is http://magixilluminate.com, another resource for learning about Magix is its youtube channel at https://www.youtube.com/MareMara13

If you have questions, you can also email the author at thomas@magixilluminate.com

Magix is Open Source, Free Software, Free as in Freedom and Free in Free Beer. The License for Magix is MITx11


##############################################################################################
# License
##############################################################################################

copyright (c) 2010 – 2014 thomas hansen - thomas@magixilluminate.com

permission is hereby granted, free of charge, to any person or company, to obtain a copy of this software and associated documentation files (the “software”), to deal in the software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the software, and to permit persons to whom the software is furnished to do so, subject to the following conditions:

the above copyright notice and this permission notice shall be included in all copies or substantial portions of the software

the software is provided “as is”, without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement.  in no event shall the authors or copyright holders be liable for any claim, damage or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software
