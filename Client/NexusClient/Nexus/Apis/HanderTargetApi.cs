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

using NexusClient.Converters;
using NexusClient.HandlerGroups;

namespace NexusClient.Nexus.Apis
{
	public class HandlerTargetApi<TCnv, TSer, TDes, TDto> : TargetApi<TCnv, TSer, TDes, TDto>
		where TCnv : IConverter<TDto>
		where TSer : IMessageSer<TDto>
		where TDes : IMessageDes<TDto>
		where TDto : IMessageDto
	{
		protected HandlerGroup<TCnv, TSer, TDes, TDto> HandlerGroup { get; set; }

		internal HandlerTargetApi(Nexus<TCnv, TSer, TDes, TDto> nexus,
			HandlerGroup<TCnv, TSer, TDes, TDto> handlerGroup) : base(nexus)
		{
			HandlerGroup = handlerGroup;
		}

		public MessageApi<TCnv, TSer, TDes, TDto> ToAll()
		{
			return To(HandlerGroup.Participants);
		}

		public MessageApi<TCnv, TSer, TDes, TDto> ToAllExcept(params string[] userIds)
		{
			return ToAllExcept(HandlerGroup.Participants, userIds);
		}

		public MessageApi<TCnv, TSer, TDes, TDto> ToOthers()
		{
			return ToOthers(HandlerGroup.Participants);
		}

		public MessageApi<TCnv, TSer, TDes, TDto> ToOthersExcept(params string[] userIds)
		{
			return ToOthersExcept(HandlerGroup.Participants, userIds);
		}
	}
}