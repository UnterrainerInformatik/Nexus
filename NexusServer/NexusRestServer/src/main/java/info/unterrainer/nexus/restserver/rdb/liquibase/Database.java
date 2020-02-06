package info.unterrainer.nexus.restserver.rdb.liquibase;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.util.Properties;

import javax.persistence.EntityManagerFactory;
import javax.persistence.Persistence;

import com.google.common.collect.ImmutableMap;

import liquibase.Contexts;
import liquibase.LabelExpression;
import liquibase.database.DatabaseFactory;
import liquibase.database.jvm.JdbcConnection;
import liquibase.exception.LiquibaseException;
import liquibase.resource.ClassLoaderResourceAccessor;
import lombok.experimental.UtilityClass;

@UtilityClass
public class Database {

	public static EntityManagerFactory startup(String persistenceUnit, String url, String user, String password) throws LiquibaseException, SQLException {

		try (Connection c = DriverManager.getConnection(url, user, password)) {
			liquibase.database.Database db = DatabaseFactory.getInstance().findCorrectDatabaseImplementation(new JdbcConnection(c));
			liquibase.Liquibase l = new liquibase.Liquibase("db/db.changelog.master.xml", new ClassLoaderResourceAccessor(), db);
			l.update(new Contexts(), new LabelExpression());
		}

		return Persistence.createEntityManagerFactory(persistenceUnit, createProperties(url, user, password));
	}

	private static Properties createProperties(String url, String user, String password) {
		Properties p = new Properties();
		p.putAll(ImmutableMap.of("javax.persistence.jdbc.url", url, "javax.persistence.jdbc.user", user, "javax.persistence.jdbc.password", password));
		return p;
	}
}
