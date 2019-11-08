package info.unterrainer.nexus.restserver;

import java.io.IOException;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import io.javalin.Context;
import lombok.Data;
import lombok.NonNull;
import lombok.RequiredArgsConstructor;

@RequiredArgsConstructor
public class Utils {

	@Data
	public class ErrorJson {
		private final String message;
		private final String exceptionMessage;

		public boolean hasValidContent() {
			return message != null || exceptionMessage != null;
		}
	}

	@NonNull
	private ObjectMapper jsonMapper;

	public void sendError(Context ctx, int code) {
		sendError(ctx, code, null, null);
	}

	public void sendError(Context ctx, int code, Exception e) {
		sendError(ctx, code, null, e);
	}

	public void sendError(Context ctx, int code, String message) {
		sendError(ctx, code, message, null);
	}

	public void sendError(Context ctx, int code, String message, Exception exception) {
		ErrorJson errorJson = new ErrorJson(message, exception.getMessage());
		try {
			if (errorJson.hasValidContent()) {
				render(ctx, errorJson, code);
			} else {
				ctx.response().sendError(code);
			}
		} catch (IOException e) {
			ctx.status(500);
		}
	}

	public <T> void render(Context ctx, T dto) {
		render(ctx, dto, 200);
	}

	public <T> void render(Context ctx, T dto, int code) {
		try {
			ctx.result(jsonMapper.writeValueAsString(dto)).contentType("application/json").status(code);
		} catch (JsonProcessingException e) {
			ctx.status(500);
		}
	}
}
