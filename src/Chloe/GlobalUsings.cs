global using System;
global using System.Text;
global using System.Linq;
global using System.IO;
global using System.Collections.Generic;

#if netfx
global using BoolResultTask = System.Threading.Tasks.Task<bool>;
global using ObjectResultTask = System.Threading.Tasks.Task<object>;
global using ValueTask = System.Threading.Tasks.Task;
#else
global using BoolResultTask = System.Threading.Tasks.ValueTask<bool>;
global using ObjectResultTask = System.Threading.Tasks.ValueTask<object>;
#endif
