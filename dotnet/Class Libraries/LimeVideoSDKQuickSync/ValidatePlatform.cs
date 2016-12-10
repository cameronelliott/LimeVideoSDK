// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if !X64PLATFORM
#error Software only builds under Active Solution Platform x64
#endif

#if DEBUG
#warning You must build Release, not Debug to get the Native DLL built
#endif