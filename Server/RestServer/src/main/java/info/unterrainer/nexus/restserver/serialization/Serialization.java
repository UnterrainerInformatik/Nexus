package info.unterrainer.nexus.restserver.serialization;

import com.fasterxml.jackson.annotation.JsonAutoDetect.Visibility;
import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.PropertyAccessor;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.dataformat.xml.XmlMapper;
import com.fasterxml.jackson.module.paramnames.ParameterNamesModule;

import lombok.experimental.UtilityClass;

@UtilityClass
public class Serialization {

	public ObjectMapper getJsonMapper() {
		ObjectMapper m = new ObjectMapper();
		setProperties(m);
		return m;
	}

	public XmlMapper getXmlMapper() {
		XmlMapper m = new XmlMapper();
		setProperties(m);
		return m;
	}

	private void setProperties(ObjectMapper m) {
		m.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
		m.configure(DeserializationFeature.READ_UNKNOWN_ENUM_VALUES_USING_DEFAULT_VALUE, true);
		m.setSerializationInclusion(JsonInclude.Include.NON_EMPTY);
		m.setVisibility(PropertyAccessor.FIELD, Visibility.ANY);
		m.registerModule(new ParameterNamesModule());
	}
}
