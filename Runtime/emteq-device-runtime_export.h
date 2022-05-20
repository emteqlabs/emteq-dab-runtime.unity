
#ifndef EMTEQ_DEVICE_RUNTIME_EXPORT_H
#define EMTEQ_DEVICE_RUNTIME_EXPORT_H

#ifdef EMTEQ_DEVICE_RUNTIME_STATIC_DEFINE
#  define EMTEQ_DEVICE_RUNTIME_EXPORT
#  define EMTEQ_DEVICE_RUNTIME_NO_EXPORT
#else
#  ifndef EMTEQ_DEVICE_RUNTIME_EXPORT
#    ifdef emteq_device_runtime_EXPORTS
        /* We are building this library */
#      define EMTEQ_DEVICE_RUNTIME_EXPORT __attribute__((visibility("default")))
#    else
        /* We are using this library */
#      define EMTEQ_DEVICE_RUNTIME_EXPORT __attribute__((visibility("default")))
#    endif
#  endif

#  ifndef EMTEQ_DEVICE_RUNTIME_NO_EXPORT
#    define EMTEQ_DEVICE_RUNTIME_NO_EXPORT __attribute__((visibility("hidden")))
#  endif
#endif

#ifndef EMTEQ_DEVICE_RUNTIME_DEPRECATED
#  define EMTEQ_DEVICE_RUNTIME_DEPRECATED __attribute__ ((__deprecated__))
#endif

#ifndef EMTEQ_DEVICE_RUNTIME_DEPRECATED_EXPORT
#  define EMTEQ_DEVICE_RUNTIME_DEPRECATED_EXPORT EMTEQ_DEVICE_RUNTIME_EXPORT EMTEQ_DEVICE_RUNTIME_DEPRECATED
#endif

#ifndef EMTEQ_DEVICE_RUNTIME_DEPRECATED_NO_EXPORT
#  define EMTEQ_DEVICE_RUNTIME_DEPRECATED_NO_EXPORT EMTEQ_DEVICE_RUNTIME_NO_EXPORT EMTEQ_DEVICE_RUNTIME_DEPRECATED
#endif

#if 0 /* DEFINE_NO_DEPRECATED */
#  ifndef EMTEQ_DEVICE_RUNTIME_NO_DEPRECATED
#    define EMTEQ_DEVICE_RUNTIME_NO_DEPRECATED
#  endif
#endif

#endif /* EMTEQ_DEVICE_RUNTIME_EXPORT_H */
