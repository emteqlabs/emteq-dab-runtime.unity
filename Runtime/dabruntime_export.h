
#ifndef DABRUNTIME_EXPORT_H
#define DABRUNTIME_EXPORT_H

#ifdef DABRUNTIME_STATIC_DEFINE
#  define DABRUNTIME_EXPORT
#  define DABRUNTIME_NO_EXPORT
#else
#  ifndef DABRUNTIME_EXPORT
#    ifdef dabRuntime_EXPORTS
        /* We are building this library */
#      define DABRUNTIME_EXPORT __attribute__((visibility("default")))
#    else
        /* We are using this library */
#      define DABRUNTIME_EXPORT __attribute__((visibility("default")))
#    endif
#  endif

#  ifndef DABRUNTIME_NO_EXPORT
#    define DABRUNTIME_NO_EXPORT __attribute__((visibility("hidden")))
#  endif
#endif

#ifndef DABRUNTIME_DEPRECATED
#  define DABRUNTIME_DEPRECATED __attribute__ ((__deprecated__))
#endif

#ifndef DABRUNTIME_DEPRECATED_EXPORT
#  define DABRUNTIME_DEPRECATED_EXPORT DABRUNTIME_EXPORT DABRUNTIME_DEPRECATED
#endif

#ifndef DABRUNTIME_DEPRECATED_NO_EXPORT
#  define DABRUNTIME_DEPRECATED_NO_EXPORT DABRUNTIME_NO_EXPORT DABRUNTIME_DEPRECATED
#endif

#if 0 /* DEFINE_NO_DEPRECATED */
#  ifndef DABRUNTIME_NO_DEPRECATED
#    define DABRUNTIME_NO_DEPRECATED
#  endif
#endif

#endif /* DABRUNTIME_EXPORT_H */
