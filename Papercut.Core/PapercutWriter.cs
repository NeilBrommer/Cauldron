using System;

namespace Papercut.Core;

public class PapercutWriter
{
	private Func<object, Task> OutputFunc { get; set; }

	public PapercutWriter(Func<object, Task> outputFunc)
	{
		this.OutputFunc = outputFunc;
	}

	public async Task Dump(object obj)
	{
		await this.OutputFunc(obj);
	}
}
