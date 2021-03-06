﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetTor.ControlPort
{
	/// <summary>
	/// A class containing the base methods and properties for a command which will be executed across a control connection,
	/// and will return a response corresponding to the response of the tor application.
	/// </summary>
	internal abstract class Command<T> where T : CommandResponse
	{
		/// <summary>
		/// Creates a new <typeparamref name="TCommand"/> object instance and dispatches the command to the specified client.
		/// </summary>
		/// <typeparam name="TCommand">The type of the command.</typeparam>
		/// <typeparam name="TResponse">The type of the response generated from the command.</typeparam>
		/// <param name="client">The client hosting the control connection port.</param>
		/// <returns><c>true</c> if the command was created and dispatched successfully; otherwise, <c>false</c>.</returns>
		public static bool DispatchAndReturn<TCommand>(string address, int controlPort, string password) where TCommand : Command<T>
		{
			try
			{
				TCommand command = Activator.CreateInstance<TCommand>();

				if (command == null)
					return false;

				T response = command.Dispatch(address, controlPort, password);
				return response.Success;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Dispatches the command to the client control port and produces a <typeparamref name="T"/> response result.
		/// </summary>
		/// <param name="client">The client hosting the control connection port.</param>
		/// <returns>A <typeparamref name="T"/> object instance containing the response data.</returns>
		public T Dispatch(string address, int controlPort, string password)
		{
			try
			{
				using (Connection connection = new Connection(address, controlPort))
				{
					if (!connection.Connect())
						throw new Exception("A command could not be dispatched to a client because the command failed to connect to the control port");

					if (!connection.Authenticate(password))
						throw new Exception("A command could not be dispatched to a client because the control could not be authenticated");

					return Dispatch(connection);
				}
			}
			catch (Exception exception)
			{
				throw new Exception("A command could not be dispatched to a client because an error occurred", exception);
			}
		}

		/// <summary>
		/// Dispatches the command to the client control port and produces a <typeparamref name="T"/> response result.
		/// </summary>
		/// <param name="connection">The control connection where the command should be dispatched.</param>
		/// <returns>A <typeparamref name="T"/> object instance containing the response data.</returns>
		protected abstract T Dispatch(Connection connection);
	}
	/// <summary>
	/// A class containing information regarding the response received back through the control connection after receiving a command from a client.
	/// </summary>
	internal class CommandResponse
	{
		private readonly bool success;

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandResponse"/> class.
		/// </summary>
		/// <param name="success">A value indicating whether the command was received and processed successfully.</param>
		public CommandResponse(bool success)
		{
			this.success = success;
		}

		#region Properties

		/// <summary>
		/// Gets a value indicating whether the command was received and processed successfully.
		/// </summary>
		public bool Success
		{
			get { return success; }
		}

		#endregion
	}

	/// <summary>
	/// A class containing a collection of mapped, <see cref="System.String"/> to <see cref="System.String"/> key-value pairs, as
	/// expected from certain commands dispatched to a control connection.
	/// </summary>
	internal sealed class ResponsePairs : Dictionary<string, string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResponsePairs"/> class.
		/// </summary>
		public ResponsePairs()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResponsePairs"/> class.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
		public ResponsePairs(int capacity) : base(capacity)
		{
		}
	}
}
