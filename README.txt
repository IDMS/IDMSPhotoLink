IDMSPhotoLink
=============

A sample project showing how to use Azure Services from COBOL programs running under CA-IDMS IDMS/DC.

For mainframe setup instructions, look inside the IDMSPhotoLink/Mainframe folder.  Everything you need should be in the zip file located there.

Clone the repository and build the solution using Visual Studio 2013.  You will likely need to download/update your Nuget packages.

You need a Microsoft Azure Service Bus account.  If you sign up for an Azure account, please reach out to us 
at support@obj-ex.com.  We will be happy to support you as your Microsoft Partner in this regard.

Note that you must update config files to include your Service Bus Namespace and access key.  Search files for the word
				"Endpoint=sb:" to find the entries.  There is one in IDMSPhotoLink and another in IDMSPhotoLink.SendTest

IDMSPhotoLink - the core web site and web service project folder.  Images are stored in the Photos folder.  
				They follow a naming convention nnnn.jpg where nnnn is the zero filled employee number.
				
				
IDMSPhotoLink.SendTest - a simple command line program to build test messages.  Allows developers to build and test new 
							version without needing to cycle through the mainframe side.  Any changes in message format should
							must also be implemented on the mainframe side.

There is no support for this project but if you want to engage with ObjEx to build production quality systems, please reach out to us at support@obj-ex.com.
