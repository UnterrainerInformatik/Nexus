/**
 * Copyright by FRUX Technologies GmbH <office@frux.io>
 */
package info.unterrainer.nexus.restserver;

import java.net.URISyntaxException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.sql.SQLException;

import javax.persistence.EntityManagerFactory;

import com.fasterxml.jackson.databind.ObjectMapper;

import info.unterrainer.nexus.restserver.rdb.liquibase.Database;
import info.unterrainer.nexus.restserver.serialization.Serialization;
import liquibase.exception.LiquibaseException;
import ma.glasnost.orika.MapperFactory;
import ma.glasnost.orika.impl.DefaultMapperFactory;

public class Main {

	private static final String PWD_KEYSTORE = "dummypwd";
	private static final String PWD_CERT = "dummypwd";

	public static void main(String[] args) throws LiquibaseException, SQLException {
		MapperFactory o = new DefaultMapperFactory.Builder().build();
		EntityManagerFactory f = Database.startup("nexus", "jdbc:mariadb://localhost:3306/nexus", "root", "test");

		ObjectMapper jsonMapper = Serialization.getJsonMapper();
		ObjectMapper xmlMapper = Serialization.getXmlMapper();
		KeystoreConfig ks = new KeystoreConfig(getKeystorePath(), "dummypwd");

		RestServer s = RestServer.create("nexus", 8080, 8443, ks, jsonMapper, xmlMapper);

		addShutdownHook(f);
		s.start();
	}

	private static Path getKeystorePath() {
		try {
			return Paths.get(Main.class.getClassLoader().getResource("keystore.jks").toURI());
		} catch (URISyntaxException e) {
			return null;
		}
	}

	private static void addShutdownHook(EntityManagerFactory f, Runnable... actions) {
		Runtime.getRuntime().addShutdownHook(new Thread(() -> {
			for (Runnable r : actions) {
				r.run();
			}
			f.close();
		}));
	}
}
