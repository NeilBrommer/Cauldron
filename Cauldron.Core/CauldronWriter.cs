using System;

namespace Cauldron.Core;

public class CauldronWriter
{
	private Func<object, Task> OutputFunc { get; set; }

	public CauldronWriter(Func<object, Task> outputFunc)
	{
		this.OutputFunc = outputFunc;
	}

	public async Task Dump(object obj)
	{
		await this.OutputFunc(obj);
	}
}
