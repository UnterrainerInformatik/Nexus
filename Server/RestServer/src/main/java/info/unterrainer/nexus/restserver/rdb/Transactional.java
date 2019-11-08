package info.unterrainer.nexus.restserver.rdb;

import java.util.function.Consumer;
import java.util.function.Function;

import javax.persistence.EntityManager;
import javax.persistence.EntityManagerFactory;

import lombok.experimental.UtilityClass;

@UtilityClass
public class Transactional {

	public static void with(EntityManagerFactory f, Consumer<EntityManager> consumer) {
		withResult(f, m -> {
			consumer.accept(m);
			return null;
		});
	}

	public static <T> T withResult(EntityManagerFactory f, Function<EntityManager, T> function) {
		if (f == null) {
			throw new IllegalArgumentException("The entityManagerFactory cannot be null.");
		}

		EntityManager m = f.createEntityManager();
		try {
			m.getTransaction().begin();
			T result = function.apply(m);
			if (m.getTransaction().isActive()) {
				m.getTransaction().commit();
			}
			return result;
		} catch (Exception e) {
			if (m.getTransaction().isActive()) {
				m.getTransaction().rollback();
			}
			throw e;
		} finally {
			m.close();
		}
	}
}
