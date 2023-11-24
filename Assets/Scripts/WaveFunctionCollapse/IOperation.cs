using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOperation
{
	public string DebugIdentifier();
	public bool Execute();
}

