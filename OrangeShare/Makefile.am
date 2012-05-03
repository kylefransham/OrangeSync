SUBDIRS =	Linux

ASSEMBLY = OrangeShare
TARGET = exe

LINK = $(REF_SPARKLESHARE) $(NOTIFY_SHARP_LIBS) $(WEBKIT_SHARP_LIBS)

if HAVE_APP_INDICATOR
BUILD_DEFINES="-define:HAVE_APP_INDICATOR"
endif

SOURCES =  \
	Program.cs \
	OrangeAboutController.cs \
	OrangeBubblesController.cs \
	OrangeControllerBase.cs \
	OrangeEventLogController.cs \
	OrangeExtensions.cs \
	OrangeInvite.cs \
	OrangeOptions.cs \
	OrangePlugin.cs \
	OrangeSetupController.cs \
	OrangeStatusIconController.cs

include $(top_srcdir)/build/build.mk
	
install-data-hook:
	test -z $(DESTDIR) && update-desktop-database $(datadir)/applications || :
