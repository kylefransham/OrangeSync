basedirs = build OrangeLib OrangeLib/Git data po

SUBDIRS = $(basedirs) $(GUISUBDIRS)
DIST_SUBDIRS = $(basedirs) OrangeShare

EXTRA_DIST = \
	gnome-doc-utils.make \
	LICENSE \
	TRADEMARK \
	AUTHORS

DISTCLEANFILES = \
	intltool-extract        \
	intltool-merge          \
	intltool-update		\
	gnome-doc-utils.make

clean-local:
	rm -rf $(top_builddir)/bin
