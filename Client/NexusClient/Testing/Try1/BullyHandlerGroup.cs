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
using Microsoft.Xna.Framework;
using Serilog;

namespace NexusClient.Testing.Try1
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
	public enum BullyMessage
	{
		TEAM_BULLY_ELECTION_CALL,
		TEAM_BULLY_ELECTION_ANSWER,
		TEAM_BULLY_VICTORY_DISTRIBUTION
	}

	public class BullyHandlerGroup : HandlerGroup
	{
		private readonly Timer.Timer electionStartedTimer = new Timer.Timer(2000f).SetIsActive(false);

		private readonly Timer.Timer waitingForVictoryMessageTimer =
			new Timer.Timer(2000f).SetIsActive(false);

		/// <summary>
		///     This handler helps electing a leader in a distributed environment.
		///     It implements a Bully-algorithm with the smallest ID winning the elections instead of the biggest one.
		/// </summary>
		public BullyHandlerGroup(string localUserId)
		{
			this.localUserId = localUserId;
			MessageHandlers.Add(BullyMessage.TEAM_BULLY_ELECTION_CALL, BullyElectionCallReceived);
			MessageHandlers.Add(BullyMessage.TEAM_BULLY_ELECTION_ANSWER, BullyElectionCallAnswerReceived);
			MessageHandlers.Add(BullyMessage.TEAM_BULLY_VICTORY_DISTRIBUTION, BullyVictoryDistributionReceived);
		}

		public string LeaderId { get; set; }
		private readonly string localUserId;

		public override void Update(GameTime gt)
		{
			lock (LockObject)
			{
				base.Update(gt);

				if (electionStartedTimer.Update(gt))
				{
					// No answer to our election-call. We obviously are the lowest ID.
					electionStartedTimer.Reset().SetIsActive(false);
					AnnounceVictory();
				}

				if (waitingForVictoryMessageTimer.Update(gt))
				{
					// No victory message received after getting an answer from potential leader.
					waitingForVictoryMessageTimer.Reset().SetIsActive(false);
					StartBullyElection();
				}
			}
		}

		private void BullyElectionCallReceived(Message message)
		{
			lock (LockObject)
			{
				var id = message.Reader.ReadString();

				if (string.Compare(localUserId, id, StringComparison.InvariantCulture) < 0)
				{
					SendBullyMessageTo(message.Sender, BullyMessage.TEAM_BULLY_ELECTION_ANSWER, localUserId);
					StartBullyElection();
					Log.Debug($"Bully-ElectionCall received from userId [{id}] - " +
								$"own ID is smaller [{localUserId}]; answering and restarting election process.");
				}
				else
				{
					waitingForVictoryMessageTimer.SetIsActive(true);
					Log.Debug($"Bully-ElectionCall received from userId [{id}] - " +
								$"own ID is bigger [{localUserId}]; new leader apprentice; waiting for victory message.");
				}
			}
		}

		private void BullyElectionCallAnswerReceived(Message message)
		{
			lock (LockObject)
			{
				var id = message.Reader.ReadString();

				electionStartedTimer.Reset().SetIsActive(false);
				if (string.Compare(localUserId, id, StringComparison.InvariantCulture) < 0)
					Log.Debug($"Bully-ElectionAnswer received from userId [{id}] - " +
								$"own ID is smaller [{localUserId}]; different election; keeping quiet.");
				else
					Log.Debug($"Bully-ElectionAnswer received from userId [{id}] - " +
								$"own ID is bigger [{localUserId}]; .");
			}
		}

		private void BullyVictoryDistributionReceived(Message message)
		{
			lock (LockObject)
			{
				if (message.Sender == null)
					return;

				var id = message.Reader.ReadString();

				if (string.Compare(localUserId, id, StringComparison.InvariantCulture) < 0)
				{
					StartBullyElection();
					Log.Debug($"Bully-VictoryDistribution received from [{message.Sender}] for userId [{id}] - " +
								$"own ID is smaller [{localUserId}]; starting election");
				}
				else
				{
					LeaderId = message.Sender;
					Log.Debug($"Bully-VictoryDistribution received from [{message.Sender}] for userId [{id}] - " +
								$"[{message.Sender}] is now the leader.");
				}
			}
		}

		public void StartBullyElection()
		{
			// Reset timers.
			electionStartedTimer.Reset().SetIsActive(false);
			waitingForVictoryMessageTimer.Reset().SetIsActive(false);

			LeaderId = localUserId;
			if (GetLowestIdUser() == localUserId)
			{
				// If we have the lowest ID, we broadcast victory immediately.
				AnnounceVictory();
				return;
			}

			// Broadcast an election call to all potential leaders.
			lock (LockObject)
			{
				foreach (var client in GetOthersWithLowerId())
					SendBullyMessageTo(client, BullyMessage.TEAM_BULLY_ELECTION_CALL, localUserId);
				electionStartedTimer.SetIsActive(true);
			}
		}

		private void AnnounceVictory()
		{
			LeaderId = localUserId;
			SendIdViaTypeToOthers(SendBullyMessageTo, BullyMessage.TEAM_BULLY_VICTORY_DISTRIBUTION, localUserId);
		}

		public string GetLowestIdUser()
		{
			lock (LockObject)
			{
				var winnerId =
					teamMessageHandler.Others.Values.SingleOrDefault(p =>
						p.Id == teamMessageHandler.Others.Values.Min(q => q.Id)) ??
					localUserId;
				if (string.Compare(localUserId, winnerId, StringComparison.InvariantCulture) < 0) winnerId = localUserId;
				return winnerId;
			}
		}

		public IEnumerable<MeshUser> GetOthersWithLowerId()
		{
			lock (LockObject)
			{
				return teamMessageHandler.Others.Values.Where(e => e.Id < localUserId);
			}
		}

		private void SendBullyMessageTo(string recipientId, BullyMessage type, string bullyId)
		{
			lock (LockObject)
			{
				Message.To(recipientId).WithContent(null).Send();
				var m = Messages.Create(type);
				m.Write(bullyId);
				Messages.Send(recipientId, P2PSendType.RELIABLE);
			}
		}

		private void SendIdViaTypeToOthers(SendIdViaTypeToOthersDelegate del, BullyMessage type, string userId)
		{
			lock (LockObject)
			{
				foreach (var client in teamMessageHandler.Others.Values) del(client, type, userId);
			}
		}

		private delegate void SendIdViaTypeToOthersDelegate(string recepientId, BullyMessage type, string id);
	}
}