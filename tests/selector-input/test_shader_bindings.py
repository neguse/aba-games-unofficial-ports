#!/usr/bin/env python3
"""Check the production collection shader's Lub/Vulkan descriptor ABI without a GPU."""
import argparse
import ctypes as C
from pathlib import Path
import struct

ROOT = Path(__file__).resolve().parents[2]


class Blob(C.Structure):
    _fields_ = [('data', C.c_void_p), ('size', C.c_size_t)]


class UniformMember(C.Structure):
    _fields_ = [('name', C.c_char * 32), ('offset_floats', C.c_int), ('comp_count', C.c_int)]


class UniformBlock(C.Structure):
    _fields_ = [('name', C.c_char * 32), ('slot', C.c_int), ('stage', C.c_int),
                ('size_floats', C.c_int), ('member_count', C.c_int), ('members', UniformMember * 32)]


class Texture(C.Structure):
    _fields_ = [('name', C.c_char * 32), ('img_slot', C.c_int), ('smp_slot', C.c_int), ('stage', C.c_int)]


class StorageBuffer(C.Structure):
    _fields_ = [('name', C.c_char * 32), ('slot', C.c_int), ('stage', C.c_int),
                ('readonly', C.c_bool), ('elem_stride', C.c_int)]


class StorageTexture(C.Structure):
    _fields_ = [('name', C.c_char * 32), ('slot', C.c_int), ('stage', C.c_int),
                ('access_format', C.c_int), ('readonly', C.c_bool)]


class Reflection(C.Structure):
    # Mirrors src/shader.h in the pinned, patched Lub checkout.
    _fields_ = [('ub_count', C.c_int), ('ubs', UniformBlock * 2),
                ('tex_count', C.c_int), ('texs', Texture * 8),
                ('is_compute', C.c_bool), ('workgroup', C.c_int * 3),
                ('storage_buf_count', C.c_int), ('storage_bufs', StorageBuffer * 4),
                ('storage_tex_count', C.c_int), ('storage_texs', StorageTexture * 4)]


def inspect(blob):
    raw = C.string_at(blob.data, blob.size)
    words = struct.unpack('<' + 'I' * (len(raw) // 4), raw)
    assert words[0] == 0x07230203, 'Compiler did not return SPIR-V'
    names, bindings, types, variables = {}, {}, {}, {}
    offset = 5
    while offset < len(words):
        count, opcode = words[offset] >> 16, words[offset] & 0xffff
        assert count and offset + count <= len(words), 'Malformed SPIR-V instruction'
        args = words[offset + 1:offset + count]
        if opcode == 5:  # OpName
            names[args[0]] = struct.pack('<' + 'I' * len(args[1:]), *args[1:]).split(b'\0')[0].decode()
        elif opcode == 71 and args[1] in (33, 34):  # OpDecorate Binding / DescriptorSet
            bindings.setdefault(args[0], {})[args[1]] = args[2]
        elif opcode in (25, 26, 27, 32):  # Image, Sampler, SampledImage, Pointer
            types[args[0]] = (opcode, args[1:])
        elif opcode == 59:  # OpVariable
            variables[args[1]] = args[0]
        offset += count
    return names, bindings, types, variables


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--library', type=Path, required=True, help='Patched liblub.so or liblub_session_test_host.so')
    args = parser.parse_args()
    lib = C.CDLL(str(args.library.resolve()))
    lib.shader_compile.argtypes = [C.c_char_p, C.c_char_p, C.c_int,
                                   C.POINTER(Blob), C.POINTER(Blob), C.POINTER(Reflection), C.c_char_p, C.c_size_t]
    lib.shader_compile.restype = C.c_bool
    lib.shader_blob_free.argtypes = [C.POINTER(Blob)]
    source = (ROOT / 'native/collection.slang').read_bytes()
    vertex, fragment, reflection, error = Blob(), Blob(), Reflection(), C.create_string_buffer(16384)
    assert lib.shader_compile(source, source, 1, C.byref(vertex), C.byref(fragment),
                              C.byref(reflection), error, len(error)), error.value.decode()
    try:
        for label, blob, uniform_set, image_set in [('vertex', vertex, 1, 0), ('fragment', fragment, 3, 2)]:
            names, bindings, types, variables = inspect(blob)
            for identifier, binding in bindings.items():
                name = names.get(identifier, str(identifier))
                assert name in ('u', 'screen'), f'{label}: unexpected descriptor {name}: {binding}'
                expected_set = uniform_set if name == 'u' else image_set
                assert binding == {33: 0, 34: expected_set}, f'{label}: {name} has wrong bindings: {binding}'
                if name == 'screen':
                    pointer_opcode, pointer = types[variables[identifier]]
                    assert pointer_opcode == 32, f'{label}: screen does not use a pointer type'
                    assert types[pointer[1]][0] == 27, f'{label}: screen must be a combined sampled image for Lub Vulkan/SDLGPU'
            assert any(names.get(identifier) == 'u' for identifier in bindings), f'{label}: missing uniform descriptor'
            if label == 'fragment':
                assert any(names.get(identifier) == 'screen' for identifier in bindings), 'Missing fragment texture'
        print('Collection shader descriptor ABI passed (actual compiler; no GPU pixels/XR validation).')
    finally:
        lib.shader_blob_free(C.byref(vertex))
        lib.shader_blob_free(C.byref(fragment))


if __name__ == '__main__':
    main()
