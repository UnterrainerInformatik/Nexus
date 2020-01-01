// ***************************************************************************
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using NexusClient.Converters.MessagePack;
using Serilog;

namespace NexusClient.HandlerGroups.Bully
{
	/// <summary>
	///     1. If P has the lowest process id, it sends a Victory message to all other processes and becomes the new
	///     Coordinator. Otherwise, P broadcasts an Election message to all other processes with lower process IDs than
	///     itself.
	///     2. If P receives no Answer after sending an Election message, then it broadcasts a Victory message to all other
	///     processes and becomes the Coordinator.
	///     3. If P receives an Answer from a process with a lower ID, it sends no further messages for this election and
	///     waits for a Victory message. (If there is no Victory message after a period of time, it restarts the process at the
	///     beginning.)
	///     4. If P receives an Election message from another process with a higher ID it sends an Answer message back and
	///     starts the election process at the beginning, by sending an Election message to lower-numbered processes.
	///     5. If P receives a Coordinator message, it treats the sender as the coordinator
	/// </summary>
	public enum BullyMessageType
	{
		BULLY_ELECTION,
		BULLY_ALIVE,
		BULLY_VICTORY
	}

	public class BullyHandlerGroup : HandlerGroup<MessagePackConverter, MessagePackSer, MessagePackDes, MessagePackDto>
	{
		public string LeaderId { get; set; }
		public bool ElectionInProgress { get; private set; }
		
		private readonly string localUserId;
		private readonly Timer.Timer electionStartedTimer = new Timer.Timer(2000f).SetIsActive(false);
		private readonly Timer.Timer waitingForVictoryMessageTimer = new Timer.Timer(4000f).SetIsActive(false);
		private readonly Timer.Timer cooldownTimer = new Timer.Timer(4000f).SetIsActive(false);
		private bool keepQuiet;
		private bool reElectSentThisUpdateCycle;
		private bool victorySentThisUpdateCycle;

		/// <summary>
		///     This handler helps electing a leader in a distributed environment.
		///     It implements a Bully-algorithm with the smallest ID winning the elections instead of the biggest one.
		/// </summary>
		public BullyHandlerGroup(string localUserId)
		{
			this.localUserId = localUserId;
			AddHandler<BullyMessage>(BullyMessageType.BULLY_ELECTION, BullyElectionReceived);
			AddHandler<BullyMessage>(BullyMessageType.BULLY_ALIVE, BullyAliveReceived);
			AddHandler<BullyMessage>(BullyMessageType.BULLY_VICTORY, BullyVictoryReceived);
		}

		public override void Update(GameTime gt)
		{
			lock (LockObject)
			{
				reElectSentThisUpdateCycle = false;
				victorySentThisUpdateCycle = false;

				if (electionStartedTimer.Update(gt))
				{
					// No answer to our election-call. We obviously are the lowest ID.
					ResetAndDisableTimers();
					AnnounceVictory();
					cooldownTimer.SetIsActive(true);
				}

				if (waitingForVictoryMessageTimer.Update(gt))
				{
					// No victory message received after getting an answer from potential leader.
					ResetAndDisableTimers();
					Start();
				}

				if (cooldownTimer.Update(gt))
				{
					cooldownTimer.Reset().SetIsActive(false);
					ElectionInProgress = false;
				}

				base.Update(gt);
			}
		}

		private void BullyElectionReceived(BullyMessage message, string senderId)
		{
			lock (LockObject)
			{
				ElectionInProgress = true;
				if (string.Compare(localUserId, senderId, StringComparison.InvariantCulture) < 0)
				{
					Log.Debug($"[{localUserId}](q={keepQuiet}): Bully-Election received from [{senderId}] - " +
							$"own ID is smaller; answering and restarting election process.");
					Message.To(senderId).Send(BullyMessageType.BULLY_ALIVE,
						new BullyMessage());
					if (!keepQuiet) Start();
				}
				else
				{
					Log.Warning($"[{localUserId}](q={keepQuiet}): Bully-Election received from [{senderId}] - " +
							$"own ID is bigger; this actually should never happen.");
				}
			}
		}

		private void BullyAliveReceived(BullyMessage message, string senderId)
		{
			lock (LockObject)
			{
				if (string.Compare(localUserId, senderId, StringComparison.InvariantCulture) < 0)
				{
					Log.Warning($"[{localUserId}](q={keepQuiet}): Bully-Alive received from [{senderId}] - " +
							$"own ID is smaller; this actually should never happen.");
				}
				else
				{
					Log.Debug($"[{localUserId}](q={keepQuiet}): Bully-Alive received from [{senderId}] - " +
							$"own ID is bigger; this comes from sending an election-call before; quietly waiting for victory-call.");
					if (!keepQuiet)
					{
						ResetAndDisableTimers();
						keepQuiet = true;
						waitingForVictoryMessageTimer.Reset().SetIsActive(true);
					}
				}
			}
		}

		private void BullyVictoryReceived(BullyMessage message, string senderId)
		{
			lock (LockObject)
			{
				ElectionInProgress = true;
				if (string.Compare(localUserId, senderId, StringComparison.InvariantCulture) < 0)
				{
					Log.Warning($"[{localUserId}](q={keepQuiet}): Bully-Victory received from [{senderId}] - " +
							$"own ID is smaller; this actually should never happen.");
				}
				else
				{
					Log.Debug($"[{localUserId}](q={keepQuiet}): Bully-Victory received from [{senderId}] - " +
							$"[{senderId}] is now the leader.");
					LeaderId = senderId;
					keepQuiet = true;
					ResetAndDisableTimers();
					cooldownTimer.Reset().SetIsActive(true);
				}
			}
		}

		private void ResetAndDisableTimers()
		{
			waitingForVictoryMessageTimer.Reset().SetIsActive(false);
			electionStartedTimer.Reset().SetIsActive(false);
		}

		public void StartBullyElection()
		{
			if (!ElectionInProgress) Start();
		}

		private void Start()
		{
			ElectionInProgress = true;
			ResetAndDisableTimers();
			keepQuiet = false;
			LeaderId = localUserId;
			lock (LockObject)
			{
				if (GetLowestIdUser() == localUserId)
				{
					AnnounceVictory();
					cooldownTimer.Reset().SetIsActive(true);
					return;
				}

				SendElectionCallToPotentialLeaders();
			}
		}

		private void SendElectionCallToPotentialLeaders()
		{
			if (reElectSentThisUpdateCycle) return;

			reElectSentThisUpdateCycle = true;
			foreach (var client in GetOthersWithLowerId())
				Message.To(client).Send(BullyMessageType.BULLY_ELECTION, new BullyMessage());
			electionStartedTimer.SetIsActive(true);
		}

		private void AnnounceVictory()
		{
			if (victorySentThisUpdateCycle) return;

			victorySentThisUpdateCycle = true;
			LeaderId = localUserId;
			Message.ToOthers().Send(BullyMessageType.BULLY_VICTORY, new BullyMessage());
		}

		public string GetLowestIdUser()
		{
			lock (LockObject)
			{
				var winnerId =
					Participants.SingleOrDefault(p => p == Participants.Min(q => q)) ??
					localUserId;
				if (string.Compare(localUserId, winnerId, StringComparison.InvariantCulture) < 0)
					winnerId = localUserId;
				return winnerId;
			}
		}

		public IEnumerable<string> GetOthersWithLowerId()
		{
			lock (LockObject)
			{
				return Participants.Where(e =>
					String.Compare(localUserId, e, StringComparison.InvariantCulture) > 0);
			}
		}
	}
}