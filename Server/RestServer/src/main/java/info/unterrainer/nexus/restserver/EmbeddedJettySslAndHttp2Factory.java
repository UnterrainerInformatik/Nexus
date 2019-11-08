/**
 * Copyright by FRUX Technologies GmbH <office@frux.io>
 */
package info.unterrainer.nexus.restserver;

import java.nio.file.Path;

import org.eclipse.jetty.alpn.server.ALPNServerConnectionFactory;
import org.eclipse.jetty.http2.HTTP2Cipher;
import org.eclipse.jetty.http2.server.HTTP2ServerConnectionFactory;
import org.eclipse.jetty.server.HttpConfiguration;
import org.eclipse.jetty.server.HttpConnectionFactory;
import org.eclipse.jetty.server.SecureRequestCustomizer;
import org.eclipse.jetty.server.Server;
import org.eclipse.jetty.server.ServerConnector;
import org.eclipse.jetty.server.SslConnectionFactory;
import org.eclipse.jetty.util.ssl.SslContextFactory;

import io.javalin.embeddedserver.jetty.EmbeddedJettyFactory;
import lombok.experimental.UtilityClass;

@UtilityClass
public class EmbeddedJettySslAndHttp2Factory {

	public static EmbeddedJettyFactory createHttp2Server(int port, int securePort, Path keystorePath, String keystorePassword) {
		return new EmbeddedJettyFactory(() -> {
			Server server = new Server();

			ServerConnector connector = new ServerConnector(server);
			connector.setPort(port);
			server.addConnector(connector);

			// HTTP Configuration.
			HttpConfiguration httpConfig = new HttpConfiguration();
			httpConfig.setSendServerVersion(false);
			httpConfig.setSecureScheme("https");
			httpConfig.setSecurePort(securePort);

			// SSL Context Factory for HTTPS and HTTP/2.
			SslContextFactory sslContextFactory = new SslContextFactory();
			sslContextFactory.setKeyStorePath(keystorePath.toString());
			sslContextFactory.setKeyStorePassword(keystorePassword);
			sslContextFactory.setCipherComparator(HTTP2Cipher.COMPARATOR);
			sslContextFactory.setProvider("Conscrypt");

			// HTTPS Configuration.
			HttpConfiguration httpsConfig = new HttpConfiguration(httpConfig);
			httpsConfig.addCustomizer(new SecureRequestCustomizer());

			// HTTP/2 Connection Factory.
			HTTP2ServerConnectionFactory h2 = new HTTP2ServerConnectionFactory(httpsConfig);
			ALPNServerConnectionFactory alpn = new ALPNServerConnectionFactory();
			alpn.setDefaultProtocol("h2");

			// SSL Connection Factory.
			SslConnectionFactory ssl = new SslConnectionFactory(sslContextFactory, alpn.getProtocol());

			// HTTP/2 Connector.
			ServerConnector http2Connector = new ServerConnector(server, ssl, alpn, h2, new HttpConnectionFactory(httpsConfig));
			http2Connector.setPort(securePort);
			server.addConnector(http2Connector);

			return server;
		});
	}
}
