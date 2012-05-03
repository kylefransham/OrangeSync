ASSEMBLY = OrangeLib
TARGET = library

SOURCES =  \
	Defines.cs \
	OrangeAnnouncement.cs \
	OrangeBackend.cs \
	OrangeChangeSet.cs \
	OrangeConfig.cs \
	OrangeExceptions.cs \
	OrangeExtensions.cs \
	OrangeFetcherBase.cs \
	OrangeHelpers.cs \
	OrangeListenerBase.cs \
	OrangeListenerFactory.cs \
	OrangeListenerTcp.cs \
	OrangeRepoBase.cs \
	OrangeUser.cs \
	OrangeWatcher.cs


install-data-hook:
	for ASM in $(EXTRA_BUNDLE); do \
		$(INSTALL) -m 0755 $$ASM $(DESTDIR)$(moduledir); \
	done;

uninstall-hook:
	for ASM in $(EXTRA_BUNDLE); do \
		rm -f $(DESTDIR)$(moduledir)/`basename $$ASM`; \
	done;

include $(top_srcdir)/build/build.mk
