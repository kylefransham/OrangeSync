SUBDIRS =	Nautilus

ASSEMBLY = OrangeShare
TARGET = exe

LINK = $(REF_SPARKLESHARE) $(NOTIFY_SHARP_LIBS) $(WEBKIT_SHARP_LIBS)

if HAVE_APP_INDICATOR
BUILD_DEFINES="-define:HAVE_APP_INDICATOR"
endif

SOURCES =  \
	../OrangeAboutController.cs \
	../OrangeBubblesController.cs \
	../OrangeControllerBase.cs \
	../OrangeEventLogController.cs \
	../OrangeExtensions.cs \
	../OrangeInvite.cs \
	../OrangeOptions.cs \
	../OrangePlugin.cs \
	../OrangeSetupController.cs \
	../OrangeStatusIconController.cs \
	../Program.cs \
	OrangeAbout.cs \
	OrangeBubbles.cs \
	OrangeController.cs \
	OrangeEventLog.cs \
	OrangeSetup.cs \
	OrangeSetupWindow.cs \
	OrangeSpinner.cs \
	OrangeStatusIcon.cs \
	OrangeUI.cs \
	OrangeUIHelpers.cs

include $(top_srcdir)/build/build.mk

bin_SCRIPTS = sparkleshare

Applicationsdir = $(datadir)/applications
dist_Applications_DATA = sparkleshare.desktop \
	sparkleshare-invite-opener.desktop
