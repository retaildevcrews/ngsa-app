// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "if Client is set within the new() block, 'docker run' command will throw System.NullReferenceException with the message 'Object reference not set to an instance of an object'", Scope = "member", Target = "~M:Ngsa.Application.DataAccessLayer.CosmosDal.#ctor(Ngsa.Application.Secrets,Ngsa.Application.Config)")]
