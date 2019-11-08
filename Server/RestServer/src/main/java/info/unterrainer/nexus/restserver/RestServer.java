package info.unterrainer.nexus.restserver;

import com.fasterxml.jackson.databind.ObjectMapper;

import info.unterrainer.nexus.restserver.enums.Attribute;
import io.javalin.Context;
import io.javalin.Javalin;
import lombok.Getter;
import lombok.RequiredArgsConstructor;
import lombok.Value;
import lombok.experimental.Accessors;
import lombok.extern.log4j.Log4j;

@Log4j
@Accessors(fluent = true)
public class RestServer {

	@RequiredArgsConstructor
	@Value
	public class Message {
		private final String message;
	}

	@Getter
	private Javalin app;
	@Getter
	private ObjectMapper jsonMapper;
	@Getter
	private ObjectMapper xmlMapper;
	@Getter
	private Utils utils;

	private RestServer() {
		// NOOP
	}

	public static RestServer create(String applicationName, int port, int securePort, KeystoreConfig keystore, ObjectMapper jsonMapper,
			ObjectMapper xmlMapper) {
		RestServer s = new RestServer();
		s.jsonMapper = jsonMapper;
		s.utils = new Utils(jsonMapper);
		s.xmlMapper = xmlMapper;
		s.app = Javalin.create().defaultContentType("application/json").defaultCharacterEncoding("UTF-8").embeddedServer(
				EmbeddedJettySslAndHttp2Factory.createHttp2Server(port, securePort, keystore.path(), keystore.password()));

		s.app().before(ctx -> setServer(ctx, s));
		s.app().get("/", s::debugOut);
		s.app().exception(Exception.class, (e, ctx) -> {
			s.utils.sendError(ctx, 500, e);
			log.error(e.getMessage(), e);
		});
		return s;
	}

	private void debugOut(Context ctx) {
		RestServer s = ctx.attribute(Attribute.SERVER);
		s.utils.render(ctx, new Message("Hello!"), 200);
	}

	public void start() {
		app.start();
	}

	private static void setServer(Context ctx, RestServer server) {
		ctx.attribute(Attribute.SERVER, server);
	}
}
